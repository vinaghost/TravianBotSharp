using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerStartAdventureTask : INotificationHandler<AdventureUpdated>, INotificationHandler<AccountInit>, INotificationHandler<AccountSettingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly GetSetting _getSetting;

        public TriggerStartAdventureTask(ITaskManager taskManager, GetSetting getSetting)
        {
            _taskManager = taskManager;
            _getSetting = getSetting;
        }

        public async Task Handle(AdventureUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId);
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId);
        }

        public async Task Handle(AccountSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId);
        }

        private async Task Trigger(AccountId accountId)
        {
            var autoStartAdventure = _getSetting.BooleanByName(accountId, AccountSettingEnums.EnableAutoStartAdventure);
            if (autoStartAdventure)
            {
                if (_taskManager.IsExist<StartAdventureTask>(accountId)) return;
                await _taskManager.Add<StartAdventureTask>(accountId);
            }
            else
            {
                var task = _taskManager.Get<StartAdventureTask>(accountId);
                await _taskManager.Remove(accountId, task);
            }
        }
    }
}