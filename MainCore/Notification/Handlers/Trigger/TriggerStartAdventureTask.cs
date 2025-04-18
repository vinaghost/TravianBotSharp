using Immediate.Handlers.Shared;
using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class StartAdventureTrigger
    {
        private static async ValueTask HandleAsync(
            AccountSettingUpdated @event,
            ITaskManager taskManager, IGetSetting getSetting,
            CancellationToken cancellationToken
        )
        {
            var accountId = @event.AccountId;
            var autoStartAdventure = getSetting.BooleanByName(accountId, AccountSettingEnums.EnableAutoStartAdventure);
            if (autoStartAdventure)
            {
                if (taskManager.IsExist<StartAdventureTask>(accountId)) return;
                await taskManager.Add<StartAdventureTask>(accountId);
            }
            else
            {
                var task = taskManager.Get<StartAdventureTask>(accountId);
                await taskManager.Remove(accountId, task);
            }
        }
    }

    public class TriggerStartAdventureTask : INotificationHandler<AdventureUpdated>, INotificationHandler<AccountInit>, INotificationHandler<AccountSettingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly IGetSetting _getSetting;

        public TriggerStartAdventureTask(ITaskManager taskManager, IGetSetting getSetting)
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