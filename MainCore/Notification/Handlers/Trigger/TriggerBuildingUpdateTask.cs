using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerBuildingUpdateTask : INotificationHandler<VillageUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly IUnitOfRepository _unitOfRepository;

        public TriggerBuildingUpdateTask(ITaskManager taskManager, IUnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(VillageUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            await Trigger(accountId);
        }

        private async Task Trigger(AccountId accountId)
        {
            var autoLoadVillageBuilding = _unitOfRepository.AccountSettingRepository.GetBooleanByName(accountId, AccountSettingEnums.AutoLoadVillageBuilding);
            if (!autoLoadVillageBuilding) return;

            var villages = _unitOfRepository.VillageRepository.GetMissingBuildingVillages(accountId);

            foreach (var village in villages)
            {
                await _taskManager.AddOrUpdate<UpdateBuildingTask>(accountId, village);
            }
        }
    }
}