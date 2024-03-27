using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerSendSettlerTask : INotificationHandler<VillageSettingUpdated>, INotificationHandler<AccountInfoUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerSendSettlerTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId);
        }

        public async Task Handle(AccountInfoUpdated notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villages = _unitOfRepository.VillageRepository.Get(accountId);

            if (!_unitOfRepository.AccountInfoRepository.IsEnoughCP(accountId)) return;

            foreach (var village in villages)
            {
                await Trigger(accountId, village);
            }
        }

        private async Task Trigger(AccountId accountId, VillageId villageId)
        {
            var autoSendSettle = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.AutoSendSettle);
            if (!autoSendSettle)
            {
                var task = _taskManager.Get<SendSettlerTask>(accountId, villageId);
                await _taskManager.Remove(accountId, task);
                return;
            }

            if (!_unitOfRepository.AccountInfoRepository.IsEnoughCP(accountId)) return;
            if (_unitOfRepository.VillageRepository.GetSettlers(villageId) < 3) return;
            if (_unitOfRepository.NewVillageRepository.IsSettling(accountId, villageId)) return;

            if (_taskManager.IsExist<SendSettlerTask>(accountId, villageId)) return;
            var now = DateTime.Now;

            if (_taskManager.IsExist<UpgradeBuildingTask>(accountId, villageId))
            {
                var task = _taskManager.Get<UpgradeBuildingTask>(accountId, villageId);
                task.ExecuteAt = now.AddSeconds(1);
            }
            await _taskManager.Add<SendSettlerTask>(accountId, villageId, executeTime: now);
        }
    }
}