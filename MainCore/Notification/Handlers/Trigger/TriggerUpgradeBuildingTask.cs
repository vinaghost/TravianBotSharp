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
        private readonly IUnitOfRepository _unitOfRepository;

        public TriggerUpgradeBuildingTaskHandler(ITaskManager taskManager, IUnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            var hasBuildingJobVillages = _unitOfRepository.VillageRepository.GetHasBuildingJobVillages(accountId);
            foreach (var village in hasBuildingJobVillages)
            {
                Trigger(accountId, village);
            }
        }

        public async Task Handle(JobUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            Trigger(accountId, villageId);
        }

        public async Task Handle(CompleteImmediatelyMessage notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            Trigger(accountId, villageId);
        }

        private void Trigger(AccountId accountId, VillageId villageId)
        {
            _taskManager.AddOrUpdate<UpgradeBuildingTask>(accountId, villageId);
        }
    }
}