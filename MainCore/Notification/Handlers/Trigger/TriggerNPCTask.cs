using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerNPCTask : INotificationHandler<StorageUpdated>, INotificationHandler<VillageSettingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly IUnitOfRepository _unitOfRepository;

        public TriggerNPCTask(ITaskManager taskManager, IUnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(StorageUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            Trigger(accountId, villageId);
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            Trigger(accountId, villageId);
        }

        private void Trigger(AccountId accountId, VillageId villageId)
        {
            var autoNPCEnable = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.AutoNPCEnable);
            if (!autoNPCEnable) return;

            var granaryPercent = _unitOfRepository.StorageRepository.GetGranaryPercent(villageId);
            var autoNPCGranaryPercent = _unitOfRepository.VillageSettingRepository.GetByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);

            if (granaryPercent < autoNPCGranaryPercent) return;
            if (_taskManager.IsExist<NPCTask>(accountId, villageId)) return;

            _taskManager.Add<NPCTask>(accountId, villageId);
        }
    }
}