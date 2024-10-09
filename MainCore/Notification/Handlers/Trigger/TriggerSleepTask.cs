using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerSleepTask : INotificationHandler<AccountInit>
    {
        private readonly ITaskManager _taskManager;
        private readonly IGetSetting _getSetting;

        public TriggerSleepTask(ITaskManager taskManager, IGetSetting getSetting)
        {
            _taskManager = taskManager;
            _getSetting = getSetting;
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            await Trigger(accountId);
        }

        private async Task Trigger(AccountId accountId)
        {
            if (_taskManager.IsExist<SleepTask>(accountId)) return;
            var workTime = _getSetting.ByName(accountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            await _taskManager.Add<SleepTask>(accountId, executeTime: DateTime.Now.AddMinutes(workTime));
        }
    }
}