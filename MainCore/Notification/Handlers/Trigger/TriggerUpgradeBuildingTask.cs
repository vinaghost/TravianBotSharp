using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerUpgradeBuildingTaskHandler : INotificationHandler<AccountInit>, INotificationHandler<JobUpdated>, INotificationHandler<CompleteImmediatelyMessage>
    {
        private readonly ITaskManager _taskManager;
        private readonly GetVillage _getVillage;

        public TriggerUpgradeBuildingTaskHandler(ITaskManager taskManager, GetVillage getVillage)
        {
            _taskManager = taskManager;
            _getVillage = getVillage;
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var hasBuildingJobVillages = _getVillage.HasBuildingJob(accountId);
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