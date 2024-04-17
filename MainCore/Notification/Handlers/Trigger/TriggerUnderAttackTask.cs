using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerUnderAttackTask : INotificationHandler<VillageSettingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerUnderAttackTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await DonateResource(notification.AccountId, notification.VillageId);
            await EvadeTroop(notification.AccountId, notification.VillageId);
        }

        private async Task DonateResource(AccountId accountId, VillageId villageId)
        {
            var enable = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.EnableDonateResource);
            if (enable) return;

            var task = _taskManager.Get<DonateResourceTask>(accountId, villageId);
            await _taskManager.Remove(accountId, task);
        }

        private async Task EvadeTroop(AccountId accountId, VillageId villageId)
        {
            var enable = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.EnableEvadeTroop);
            if (enable) return;

            var task = _taskManager.Get<EvadeTroopTask>(accountId, villageId);
            await _taskManager.Remove(accountId, task);
        }
    }
}