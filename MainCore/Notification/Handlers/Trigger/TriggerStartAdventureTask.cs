﻿using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerStartAdventureTask : INotificationHandler<AdventureUpdated>, INotificationHandler<AccountInit>, INotificationHandler<AccountSettingUpdated>
    {
        private readonly ITaskManager _taskManager;


        public TriggerStartAdventureTask(ITaskManager taskManager)
        {
            _taskManager = taskManager;

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
            var autoStartAdventure = new GetSetting().BooleanByName(accountId, AccountSettingEnums.EnableAutoStartAdventure);
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