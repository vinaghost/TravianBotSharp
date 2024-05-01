using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerNPCTask : INotificationHandler<StorageUpdated>, INotificationHandler<VillageSettingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerNPCTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(StorageUpdated notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            await Trigger(accountId, villageId);
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            await Trigger(accountId, villageId);
        }

        private async Task Trigger(AccountId accountId, VillageId villageId)
        {
            var autoNPCEnable = _villageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
            if (autoNPCEnable)
            {
                var granaryPercent = _unitOfRepository.StorageRepository.GetGranaryPercent(villageId);
                var autoNPCGranaryPercent = _villageSettingRepository.GetByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);

                if (granaryPercent < autoNPCGranaryPercent) return;
                if (_taskManager.IsExist<NPCTask>(accountId, villageId)) return;

                await _taskManager.Add<NPCTask>(accountId, villageId);
            }
            else
            {
                var task = _taskManager.Get<NPCTask>(accountId, villageId);
                await _taskManager.Remove(accountId, task);
            }
        }
    }
}