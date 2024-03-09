using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerCompleteImmediatelyTask : INotificationHandler<VillageSettingUpdated>, INotificationHandler<QueueBuildingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerCompleteImmediatelyTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(QueueBuildingUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId);
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId);
        }

        private async Task Trigger(AccountId accountId, VillageId villageId)
        {
            if (_taskManager.IsExist<CompleteImmediatelyTask>(accountId, villageId)) return;
            _unitOfRepository.QueueBuildingRepository.Clean(villageId);
            var count = _unitOfRepository.QueueBuildingRepository.Count(villageId);
            if (count == 0) return;

            var completeImmediatelyEnable = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.CompleteImmediately);
            if (!completeImmediatelyEnable) return;

            var applyRomanQueueLogicWhenBuilding = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);
            var plusActive = _unitOfRepository.AccountInfoRepository.IsPlusActive(accountId);

            var countNeeded = 1;
            if (applyRomanQueueLogicWhenBuilding)
            {
                countNeeded++;
            }
            if (plusActive)
            {
                countNeeded++;
            }
            if (count != countNeeded) return;

            if (!_unitOfRepository.QueueBuildingRepository.IsSkippableBuilding(villageId)) return;

            await _taskManager.Add<CompleteImmediatelyTask>(accountId, villageId);
        }
    }
}