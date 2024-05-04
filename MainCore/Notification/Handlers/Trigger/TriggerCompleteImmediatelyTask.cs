using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerCompleteImmediatelyTask : INotificationHandler<VillageSettingUpdated>, INotificationHandler<QueueBuildingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly IVillageSettingRepository _villageSettingRepository;
        private readonly IQueueBuildingRepository _queueBuildingRepository;
        private readonly IAccountInfoRepository _accountInfoRepository;

        public TriggerCompleteImmediatelyTask(ITaskManager taskManager, IVillageSettingRepository villageSettingRepository, IQueueBuildingRepository queueBuildingRepository, IAccountInfoRepository accountInfoRepository)
        {
            _taskManager = taskManager;
            _villageSettingRepository = villageSettingRepository;
            _queueBuildingRepository = queueBuildingRepository;
            _accountInfoRepository = accountInfoRepository;
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
            _queueBuildingRepository.Clean(villageId);
            var count = _queueBuildingRepository.Count(villageId);
            if (count == 0) return;

            var completeImmediatelyEnable = new GetVillageSetting().BooleanByName(villageId, VillageSettingEnums.CompleteImmediately);
            if (!completeImmediatelyEnable) return;

            var applyRomanQueueLogicWhenBuilding = new GetVillageSetting().BooleanByName(villageId, VillageSettingEnums.ApplyRomanQueueLogicWhenBuilding);
            var plusActive = _accountInfoRepository.IsPlusActive(accountId);

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

            if (!_queueBuildingRepository.IsSkippableBuilding(villageId)) return;

            await _taskManager.Add<CompleteImmediatelyTask>(accountId, villageId);
        }
    }
}