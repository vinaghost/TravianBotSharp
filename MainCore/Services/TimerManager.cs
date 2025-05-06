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

        private bool _isShutdown = false;

        private readonly ITaskManager _taskManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private static ResiliencePropertyKey<ContextData> contextDataKey = new(nameof(ContextData));
        private readonly ResiliencePipeline<Result> _pipeline;

        public TimerManager(ITaskManager taskManager, IServiceScopeFactory serviceScopeFactory)
        {
            _taskManager = taskManager;
            _serviceScopeFactory = serviceScopeFactory;

            Func<OnRetryArguments<Result>, ValueTask> OnRetry = async static args =>
            {
                args.Context.Properties.TryGetValue(contextDataKey, out var contextData);

                var (accountId, taskName, logger, browser) = contextData;
                logger.Warning("There is something wrong.");
                var error = args.Outcome;
                if (error.Exception is null)
                {
                    var errors = error.Result.Reasons.Select(x => x.Message).ToList();
                    logger.Error("{Errors}", string.Join(Environment.NewLine, errors));
                }
                else
                {
                    var exception = error.Exception;
                    logger.Error(exception, "{Message}", exception.Message);
                }

                logger.Warning("Retry {AttemptNumber} for {TaskName}", args.AttemptNumber + 1, taskName);
                await browser.Refresh(args.Context.CancellationToken);
            };

            var retryOptions = new RetryStrategyOptions<Result>()
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(5),
                UseJitter = true,
                BackoffType = DelayBackoffType.Linear,
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

            using var scope = _serviceScopeFactory.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
            dataService.AccountId = accountId;
            var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
            logger.Information("{TaskName} is started", task.Description);

            ///===========================================================///
            var context = ResilienceContextPool.Shared.Get(cts.Token);

            var browser = scope.ServiceProvider.GetRequiredService<IChromeBrowser>();
            var contextData = new ContextData(accountId, task.Description, logger, browser);
            context.Properties.Set(contextDataKey, contextData);
            var handler = GetHandler(scope, task);
            var poliResult = await _pipeline.ExecuteOutcomeAsync(
                async (ctx, state) => Outcome.FromResult(await handler.HandleAsync(state, ctx.CancellationToken)),
                context,
                task);

            ResilienceContextPool.Shared.Return(context);
            ///===========================================================///
            logger.Information("{TaskName} is finished", task.Description);

            task.Stage = StageEnums.Waiting;
            taskQueue.IsExecuting = false;

            cts.Dispose();
            taskQueue.CancellationTokenSource = null;

            if (poliResult.Exception is not null)
            {
                logger.Warning("There is something wrong. Bot is pausing. Last exception is");
                var ex = poliResult.Exception;
                logger.Error(ex, "{Message}", ex.Message);
                await _taskManager.SetStatus(accountId, StatusEnums.Paused);
            }
            else
            {
                var result = poliResult.Result;
                if (result.IsFailed)
                {
                    var errors = result.Reasons.Select(x => x.Message).ToList();
                    logger.Warning(string.Join(Environment.NewLine, errors));

                    if (result.HasError<Stop>() || result.HasError<Retry>())
                    {
                        await _taskManager.SetStatus(accountId, StatusEnums.Paused);
                    }
                    else if (result.HasError<Skip>())
                    {
                        if (task.ExecuteAt == cacheExecuteTime)
                        {
                            await _taskManager.Remove(accountId, task);
                        }
                        else
                        {
                            await _taskManager.ReOrder(accountId);
                        }
                    }
                    else if (result.HasError<Cancel>())
                    {
                        await _taskManager.SetStatus(accountId, StatusEnums.Paused);
                    }
                }
                else
                {
                    if (task.ExecuteAt == cacheExecuteTime)
                    {
                        await _taskManager.Remove(accountId, task);
                    }
                    else
                    {
                        await _taskManager.ReOrder(accountId);
                    }
                }
            }
            var delayTaskCommand = scope.ServiceProvider.GetService<DelayTaskCommand.Handler>();
            await delayTaskCommand.HandleAsync(new(accountId));
        }

        private IHandler<T, Result> GetHandler<T>(IServiceScope scope, T task)
        {
            var handler = scope.ServiceProvider.GetRequiredService<IHandler<T, Result>>();
            return handler;
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

        public record ContextData(AccountId AccountId, string TaskName, ILogger Logger, IChromeBrowser Browser);
    }
}