using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerSleepTask : INotificationHandler<AccountInit>
    {
        private readonly ITaskManager _taskManager;

        public TriggerSleepTask(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            await Trigger(accountId);
        }

        private async Task Trigger(AccountId accountId)
        {
            if (_taskManager.IsExist<SleepTask>(accountId)) return;
            var workTime = new GetSetting().ByName(accountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            await _taskManager.Add<SleepTask>(accountId, executeTime: DateTime.Now.AddMinutes(workTime));
        }
    }
}