using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTask]
    public class SleepTask : AccountTask
    {
        private readonly ITaskManager _taskManager;
        private readonly ILogService _logService;

        public SleepTask(ITaskManager taskManager, ILogService logService)
        {
            _taskManager = taskManager;
            _logService = logService;
        }

        protected override async Task<Result> Execute()
        {
            await _chromeBrowser.Close();

            Result result;
            result = await Sleep();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var accessResult = await new GetAccess().Execute(AccountId);
            if (accessResult.IsFailed) return Result.Fail(accessResult.Errors).WithError(TraceMessage.Error(TraceMessage.Line()));
            var access = accessResult.Value;

            result = await new OpenBrowserCommand().Execute(_chromeBrowser, AccountId, access, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await SetNextExecute();

            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var workTime = new GetSetting().ByName(AccountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            ExecuteAt = DateTime.Now.AddMinutes(workTime);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            _name = "Sleep task";
        }

        private async Task<Result> Sleep()
        {
            var logger = _logService.GetLogger(AccountId);

            var sleepTimeMinutes = new GetSetting().ByName(AccountId, AccountSettingEnums.SleepTimeMin, AccountSettingEnums.SleepTimeMax);
            var sleepEnd = DateTime.Now.AddMinutes(sleepTimeMinutes);
            int lastMinute = 0;
            while (true)
            {
                if (CancellationToken.IsCancellationRequested) return Cancel.Error;
                var timeRemaining = sleepEnd - DateTime.Now;
                if (timeRemaining < TimeSpan.Zero) return Result.Ok();
                await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
                var currentMinute = (int)timeRemaining.TotalMinutes;
                if (lastMinute != currentMinute)
                {
                    logger.Information("Chrome will reopen in {CurrentMinute} mins", currentMinute);
                    lastMinute = currentMinute;
                }
            }
        }
    }
}