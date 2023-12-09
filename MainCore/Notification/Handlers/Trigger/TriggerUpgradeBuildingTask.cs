using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerUpgradeBuildingTaskHandler : INotificationHandler<AccountInit>, INotificationHandler<JobUpdated>, INotificationHandler<CompleteImmediatelyMessage>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerUpgradeBuildingTaskHandler(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var hasBuildingJobVillages = _unitOfRepository.VillageRepository.GetHasBuildingJobVillages(accountId);
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