﻿using Humanizer;
using Humanizer.Localisation;
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

                browser.Logger.Warning("{TaskName} will retry after {times} (#{AttemptNumber} times)", taskName, args.Duration.Humanize(minUnit: TimeUnit.Second), args.AttemptNumber + 1);
                await browser.Refresh(CancellationToken.None);
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
                    var filename = await browser.Screenshot();
                    logger.Information("Screenshot saved as {FileName}", filename);
                    logger.Warning("There is something wrong. Bot is pausing. Last exception is");
                    logger.Error(ex, "{Message}", ex.Message);
                }

                _taskManager.SetStatus(accountId, StatusEnums.Paused);
            }

            if (poliResult.Result is not null)
            {
                var result = poliResult.Result;
                if (result.IsFailed)
                {
                    if (result.HasError<Stop>() || result.HasError<Retry>())
                    {
                        var filename = await browser.Screenshot();
                        logger.Information(messageTemplate: "Screenshot saved as {FileName}", filename);
                        _taskManager.SetStatus(accountId, StatusEnums.Paused);
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

        public record ContextData(AccountId AccountId, string TaskName, IChromeBrowser Browser);
    }
}