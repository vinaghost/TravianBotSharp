using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Timer = System.Timers.Timer;

namespace MainCore.Services
{
    [RegisterSingleton<ITimerManager, TimerManager>]
    public sealed class TimerManager : ITimerManager
    {
        private readonly Dictionary<AccountId, Timer> _timers = [];
        private readonly Dictionary<AccountId, DateTime> _restartTimerTime = [];

        private bool _isShutdown = false;

        private readonly ITaskManager _taskManager;
        private readonly ICustomServiceScopeFactory _serviceScopeFactory;

        private static ResiliencePropertyKey<ContextData> contextDataKey = new(nameof(ContextData));
        private readonly ResiliencePipeline<Result> _pipeline;

        public TimerManager(ITaskManager taskManager, ICustomServiceScopeFactory serviceScopeFactory)
        {
            _taskManager = taskManager;
            _serviceScopeFactory = serviceScopeFactory;

            Func<OnRetryArguments<Result>, ValueTask> OnRetry = async static args =>
            {
                await Task.CompletedTask;
                if (!args.Context.Properties.TryGetValue(contextDataKey, out var contextData)) return;

                var (accountId, taskName, browser) = contextData;
                var error = args.Outcome;
                if (error.Exception is not null)
                {
                    var exception = error.Exception;
                    browser.Logger.Error(exception, "{Message}", exception.Message);
                }
                if (error.Result is not null)
                {
                    var message = string.Join(Environment.NewLine, error.Result.Reasons.Select(e => e.Message));
                    if (!string.IsNullOrEmpty(message))
                    {
                        browser.Logger.Warning("Task {TaskName} failed", taskName, message);
                        browser.Logger.Warning("{Message}", message);
                    }
                }

                browser.Logger.Warning("{TaskName} will retry after {RetryDelay} (#{AttemptNumber} times)", taskName, args.RetryDelay.Humanize(3, minUnit: Humanizer.Localisation.TimeUnit.Second), args.AttemptNumber + 1);
            };

            var retryOptions = new RetryStrategyOptions<Result>()
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(30),
                UseJitter = true,
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<Result>()
                   .Handle<Exception>()
                   .HandleResult(static x => x.HasError<Retry>()),
                OnRetry = OnRetry
            };

            _pipeline = new ResiliencePipelineBuilder<Result>()
                .AddRetry(retryOptions)
                .Build();
        }

        public async Task Execute(AccountId accountId)
        {
            var taskQueue = _taskManager.GetTaskQueue(accountId);

            var status = taskQueue.Status;

            if (status == StatusEnums.Paused)
            {
                if (_restartTimerTime.ContainsKey(accountId) &&
                    _restartTimerTime[accountId] > DateTime.Now)
                {
                    await Restart(accountId);
                    _restartTimerTime.Remove(accountId);
                    return;
                }
            }

            if (status != StatusEnums.Online) return;
            var tasks = taskQueue.Tasks;
            if (tasks.Count == 0) return;
            var task = tasks[0];

            if (task.ExecuteAt > DateTime.Now) return;

            taskQueue.IsExecuting = true;
            var cts = new CancellationTokenSource();
            taskQueue.CancellationTokenSource = cts;

            task.Stage = StageEnums.Executing;
            var cacheExecuteTime = task.ExecuteAt;
            using var scope = _serviceScopeFactory.CreateScope(accountId);

            ///===========================================================///
            var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
            var browser = scope.ServiceProvider.GetRequiredService<IChromeBrowser>();
            var logger = browser.Logger;

            var contextData = new ContextData(accountId, task.Description, browser);

            ///===========================================================///
            var context = ResilienceContextPool.Shared.Get(cts.Token);

            context.Properties.Set(contextDataKey, contextData);

            var poliResult = await _pipeline.ExecuteOutcomeAsync(
                async (ctx, state) => Outcome.FromResult(await scope.Execute(state, ctx.CancellationToken)),
                context,
                task);

            ResilienceContextPool.Shared.Return(context);
            ///===========================================================///

            task.Stage = StageEnums.Waiting;
            taskQueue.IsExecuting = false;

            cts.Dispose();
            taskQueue.CancellationTokenSource = null;

            if (poliResult.Exception is not null)
            {
                var ex = poliResult.Exception;

                if (ex is OperationCanceledException)
                {
                    logger.Information("Pause button is pressed");
                }
                else
                {
                    try
                    {
                        var filename = await browser.Screenshot();
                        logger.Information("Screenshot saved as {FileName}", filename);
                    }
                    catch { }
                    logger.Warning("There is something wrong. Bot is pausing. Last exception is");
                    logger.Error(ex, "{Message}", ex.Message);

                    logger.Warning("Restarting timer in 15 minutes.");
                    SetRestartTime(accountId, logger);
                }

                _taskManager.SetStatus(accountId, StatusEnums.Paused);
            }

            if (poliResult.Result is not null)
            {
                var result = poliResult.Result;
                if (result.IsFailed)
                {
                    var message = string.Join(Environment.NewLine, result.Reasons.Select(e => e.Message));
                    if (!string.IsNullOrEmpty(message))
                    {
                        logger.Warning("Task {TaskName} failed", task.Description, message);
                        logger.Warning("{Message}", message);
                    }

                    if (result.HasError<Stop>() || result.HasError<Retry>())
                    {
                        var filename = await browser.Screenshot();
                        logger.Information(messageTemplate: "Screenshot saved as {FileName}", filename);
                        _taskManager.SetStatus(accountId, StatusEnums.Paused);
                        SetRestartTime(accountId, logger);
                    }
                    else if (result.HasError<Skip>())
                    {
                        if (task.ExecuteAt == cacheExecuteTime)
                        {
                            _taskManager.Remove(accountId, task);
                        }
                        else
                        {
                            _taskManager.ReOrder(accountId);
                            logger.Information("Schedule next run at {Time}", task.ExecuteAt.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                    }
                    else if (result.HasError<Cancel>())
                    {
                        _taskManager.SetStatus(accountId, StatusEnums.Paused);
                    }
                }
                else
                {
                    if (task.ExecuteAt == cacheExecuteTime)
                    {
                        _taskManager.Remove(accountId, task);
                    }
                    else
                    {
                        _taskManager.ReOrder(accountId);
                        logger.Information("Schedule next run at {Time}", task.ExecuteAt.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }
            }

            var delayTaskCommand = scope.ServiceProvider.GetRequiredService<DelayTaskCommand.Handler>();
            await delayTaskCommand.HandleAsync(new(accountId));
        }

        public void Shutdown()
        {
            _isShutdown = true;
            foreach (var timer in _timers.Values)
            {
                timer.Dispose();
            }
        }

        public void Start(AccountId accountId)
        {
            if (!_timers.ContainsKey(accountId))
            {
                var timer = new Timer(100) { AutoReset = false };
                timer.Elapsed += async (sender, e) =>
                {
                    if (_isShutdown) return;
                    await Execute(accountId);
                    timer.Start();
                };

                _timers.Add(accountId, timer);
                timer.Start();
            }
        }

        private void SetRestartTime(AccountId accountId, ILogger logger)
        {
            logger.Information("Restarting timer in 15 minutes.");
            _restartTimerTime[accountId] = DateTime.Now.AddMinutes(15);
        }

        private async Task Restart(AccountId accountId)
        {
            using var scope = _serviceScopeFactory.CreateScope(accountId);

            var getAccessQuery = scope.ServiceProvider.GetRequiredService<GetValidAccessQuery.Handler>();
            var browser = scope.ServiceProvider.GetRequiredService<IChromeBrowser>();
            var logger = browser.Logger;

            await browser.Close();

            var result = await getAccessQuery.HandleAsync(new(accountId, true));
            if (result.IsFailed)
            {
                logger.Information("{message}", string.Join(", ", result.Errors.Select(x => x.Message)));
                SetRestartTime(accountId, logger);

                return;
            }

            var openBrowserCommand = scope.ServiceProvider.GetRequiredService<OpenBrowserCommand.Handler>();

            try
            {
                await openBrowserCommand.HandleAsync(new(accountId, result.Value));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to open browser: {Message}", ex.Message);
                SetRestartTime(accountId, logger);
                return;
            }

            await _taskManager.Restart(accountId);
        }

        public record ContextData(AccountId AccountId, string TaskName, IChromeBrowser Browser);
    }
}