using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerUpdateExpansionTask : INotificationHandler<BuildingUpdated>, INotificationHandler<VillageSettingUpdated>
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
            var autoTrainSettle = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.AutoTrainSettle);
            if (!autoTrainSettle)
            {
                var task = _taskManager.Get<UpdateExpansionSlotTask>(accountId, villageId);
                await _taskManager.Remove(accountId, task);
                return;
            }

            var building = _unitOfRepository.BuildingRepository.GetSettleLocation(villageId);
            if (building == default) return;

            var expansionSlotDefault = _unitOfRepository.ExpansionSlotRepository.IsDefaultExpansionSlot(villageId);
            if (!expansionSlotDefault) return;

            if (_taskManager.IsExist<UpdateExpansionSlotTask>(accountId, villageId)) return;
            await _taskManager.Add<UpdateExpansionSlotTask>(accountId, villageId);
        }
    }
}