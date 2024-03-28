using MainCore.Common.Enums;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerUpdateExpansionTask : INotificationHandler<BuildingUpdated>, INotificationHandler<VillageSettingUpdated>, INotificationHandler<QueueBuildingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerUpdateExpansionTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(BuildingUpdated notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;

            var autoTrainSettle = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.AutoTrainSettle);
            if (!autoTrainSettle)
            {
                var task = _taskManager.Get<UpdateExpansionSlotTask>(accountId, villageId);
                await _taskManager.Remove(accountId, task);
                return;
            }

            var expansionSlotDefault = _unitOfRepository.ExpansionSlotRepository.IsDefaultExpansionSlot(villageId);
            if (!expansionSlotDefault) return;

            var building = _unitOfRepository.BuildingRepository.GetSettleLocation(villageId);
            if (building == default) return;

            if (_taskManager.IsExist<UpdateExpansionSlotTask>(accountId, villageId)) return;
            await _taskManager.Add<UpdateExpansionSlotTask>(accountId, villageId);
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;

            var autoTrainSettle = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.AutoTrainSettle);
            if (!autoTrainSettle)
            {
                var task = _taskManager.Get<UpdateExpansionSlotTask>(accountId, villageId);
                await _taskManager.Remove(accountId, task);
                return;
            }

            var building = _unitOfRepository.BuildingRepository.GetSettleLocation(villageId);
            if (building == default) return;

            if (_taskManager.IsExist<UpdateExpansionSlotTask>(accountId, villageId)) return;
            await _taskManager.Add<UpdateExpansionSlotTask>(accountId, villageId);
        }

        public async Task Handle(QueueBuildingUpdated notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;

            var autoTrainSettle = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.AutoTrainSettle);
            if (!autoTrainSettle)
            {
                var task = _taskManager.Get<UpdateExpansionSlotTask>(accountId, villageId);
                await _taskManager.Remove(accountId, task);
                return;
            }

            var building = _unitOfRepository.QueueBuildingRepository.GetSettleBuilding(villageId);

            if (building is null) return;
            if (building.Level == 15 && building.Type != BuildingEnums.Palace) return;

            if (_taskManager.IsExist<UpdateExpansionSlotTask>(accountId, villageId)) return;
            await _taskManager.Add<UpdateExpansionSlotTask>(accountId, villageId, executeTime: building.CompleteTime.AddSeconds(20));
        }
    }
}