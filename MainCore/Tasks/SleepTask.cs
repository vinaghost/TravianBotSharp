using MainCore.Commands.Misc;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class SleepTask : AccountTask
    {
        private readonly GetAccessCommand _getAccessCommand;
        private readonly IAccountSettingRepository _accountSettingRepository;

        private readonly ITaskManager _taskManager;
        private readonly ILogService _logService;

        public SleepTask(IChromeManager chromeManager, IMediator mediator, GetAccessCommand getAccessCommand, IAccountSettingRepository accountSettingRepository, ITaskManager taskManager, ILogService logService) : base(chromeManager, mediator)
        {
            _getAccessCommand = getAccessCommand;
            _accountSettingRepository = accountSettingRepository;
            _taskManager = taskManager;
            _logService = logService;
        }

        protected override async Task<Result> Execute()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            await Task.Run(chromeBrowser.Close, CancellationToken.None);

            Result result;
            result = await Sleep();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var accessResult = await _getAccessCommand.Execute(AccountId);
            if (accessResult.IsFailed) return Result.Fail(accessResult.Errors).WithError(TraceMessage.Error(TraceMessage.Line()));
            var access = accessResult.Value;

            result = await _mediator.Send(new OpenBrowserCommand(AccountId, access, chromeBrowser), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await SetNextExecute();

            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var workTime = _accountSettingRepository.GetByName(AccountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
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

            var sleepTimeMinutes = _accountSettingRepository.GetByName(AccountId, AccountSettingEnums.SleepTimeMin, AccountSettingEnums.SleepTimeMax);
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
                    logger.Information("Chrome will reopen in {currentMinute} mins", currentMinute);
                    lastMinute = currentMinute;
                }
            }
        }
    }
}