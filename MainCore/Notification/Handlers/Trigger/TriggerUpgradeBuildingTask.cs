﻿using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerUpgradeBuildingTaskHandler : INotificationHandler<AccountInit>, INotificationHandler<JobUpdated>, INotificationHandler<CompleteImmediatelyMessage>
    {
        private readonly ITaskManager _taskManager;

        public TriggerUpgradeBuildingTaskHandler(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var hasBuildingJobVillages = new GetVillage().Job(accountId);
            foreach (var village in hasBuildingJobVillages)
            {
                await Trigger(accountId, village);
            }
        }

        public async Task Handle(JobUpdated notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            await Trigger(accountId, villageId);
        }

        public async Task Handle(CompleteImmediatelyMessage notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            await Trigger(accountId, villageId);
        }

        private async Task Trigger(AccountId accountId, VillageId villageId)
        {
            await _taskManager.AddOrUpdate<UpgradeBuildingTask>(accountId, villageId);
        }
    }
}