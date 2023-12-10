using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerRefreshVillageTask : INotificationHandler<VillageSettingUpdated>, INotificationHandler<AccountInit>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerRefreshVillageTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            await Trigger(accountId, villageId);
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;

            var villages = _unitOfRepository.VillageRepository.Get(accountId);
            foreach (var village in villages)
            {
                await Trigger(accountId, village);
            }
        }

        private async Task Trigger(AccountId accountId, VillageId villageId)
        {
            var autoRefreshEnable = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.AutoRefreshEnable);
            if (autoRefreshEnable)
            {
                if (_taskManager.IsExist<UpdateVillageTask>(accountId, villageId)) return;
                await _taskManager.Add<UpdateVillageTask>(accountId, villageId);
            }
            else
            {
                var task = _taskManager.Get<UpdateVillageTask>(accountId, villageId);
                await _taskManager.Remove(accountId, task);
            }
        }
    }
}