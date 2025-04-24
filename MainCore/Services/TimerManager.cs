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
        private readonly Dictionary<AccountId, TaskData> _taskData = new();
        private readonly StatusUpdated.Handler _statusUpdated;

        private bool _isShutdown = false;

        private readonly ITaskManager _taskManager;
        private readonly IChromeManager _chromeManager;
        private readonly ILogService _logService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly ResiliencePropertyKey<ILogger> contextLoggerKey = new("logger");
        private readonly ResiliencePropertyKey<string> contextTaskNameKey = new("task_name");
        private readonly ResiliencePropertyKey<AccountId> contextAccountIdKey = new("account_id");

        private readonly ResiliencePipeline<Result> _pipeline;

        public TimerManager(ITaskManager taskManager, ILogService logService, IChromeManager chromeManager, IServiceScopeFactory serviceScopeFactory, StatusUpdated.Handler statusUpdated)
        {
            _taskManager = taskManager;
            _chromeManager = chromeManager;
            _logService = logService;
            _serviceScopeFactory = serviceScopeFactory;
            _statusUpdated = statusUpdated;

            Func<OnRetryArguments<Result>, ValueTask> OnRetry = async args =>
            {
                args.Context.Properties.TryGetValue(contextLoggerKey, out var logger);
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

                args.Context.Properties.TryGetValue(contextTaskNameKey, out var taskName);
                logger.Warning("Retry {AttemptNumber} for {TaskName}", args.AttemptNumber + 1, taskName);

                args.Context.Properties.TryGetValue(contextAccountIdKey, out var accountId);
                var chromeBrowser = _chromeManager.Get(accountId);
                await chromeBrowser.Refresh(args.Context.CancellationToken);
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
            var status = GetStatus(accountId);
            if (status != StatusEnums.Online) return;
            var tasks = _taskManager.GetTaskList(accountId);
            if (tasks.Count == 0) return;
            var task = tasks[0];

            if (task.ExecuteAt > DateTime.Now) return;

            var logger = _logService.GetLogger(accountId);

            var taskData = GetTaskData(accountId);
            taskData.IsExecuting = true;
            task.Stage = StageEnums.Executing;
            var cts = new CancellationTokenSource();
            taskData.CancellationTokenSource = cts;

            var cacheExecuteTime = task.ExecuteAt;

            logger.Information("{TaskName} is started", task.GetName());
            using var scoped = _serviceScopeFactory.CreateScope();
            ///===========================================================///
            var context = ResilienceContextPool.Shared.Get(cts.Token);

            context.Properties.Set(contextLoggerKey, logger);
            context.Properties.Set(contextTaskNameKey, task.GetName());
            context.Properties.Set(contextAccountIdKey, accountId);

            var poliResult = await _pipeline.ExecuteOutcomeAsync(
                async (ctx, state) => Outcome.FromResult(await task.Handle(state, ctx.CancellationToken)),
                context,
                scoped);

            ResilienceContextPool.Shared.Return(context);
            ///===========================================================///
            logger.Information("{TaskName} is finished", task.GetName());

            task.Stage = StageEnums.Waiting;
            taskData.IsExecuting = false;

            cts.Dispose();
            taskData.CancellationTokenSource = null;

            if (poliResult.Exception is not null)
            {
                logger.Warning("There is something wrong. Bot is pausing. Last exception is");
                var ex = poliResult.Exception;
                logger.Error(ex, "{Message}", ex.Message);
                await SetStatus(accountId, StatusEnums.Paused);
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
                        await SetStatus(accountId, StatusEnums.Paused);
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
                        await SetStatus(accountId, StatusEnums.Paused);
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
            var delayTaskCommand = scoped.ServiceProvider.GetService<DelayTaskCommand>();
            await delayTaskCommand.Execute(CancellationToken.None);
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

        public class TaskData
        {
            public bool IsExecuting { get; set; } = false;
            public StatusEnums Status { get; set; } = StatusEnums.Offline;
            public CancellationTokenSource CancellationTokenSource { get; set; } = null;
        }

        private TaskData GetTaskData(AccountId accountId)
        {
            if (_taskData.ContainsKey(accountId))
            {
                return _taskData[accountId];
            }
            else
            {
                var data = new TaskData();
                _taskData.Add(accountId, data);
                return data;
            }
        }

        public StatusEnums GetStatus(AccountId accountId)
        {
            var taskInfo = GetTaskData(accountId);
            return taskInfo.Status;
        }

        public async Task SetStatus(AccountId accountId, StatusEnums status)
        {
            var taskInfo = GetTaskData(accountId);
            taskInfo.Status = status;
            await _statusUpdated.HandleAsync(new(accountId));
        }

        public bool IsExecuting(AccountId accountId)
        {
            var taskInfo = GetTaskData(accountId);
            return taskInfo.IsExecuting;
        }

        public CancellationTokenSource GetCancellationTokenSource(AccountId accountId)
        {
            var taskInfo = GetTaskData(accountId);
            return taskInfo.CancellationTokenSource;
        }
    }
}