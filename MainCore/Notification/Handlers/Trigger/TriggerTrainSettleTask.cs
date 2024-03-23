using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerTrainSettleTask : INotificationHandler<VillageSettingUpdated>, INotificationHandler<BuildingUpdated>, INotificationHandler<ExpansionSlotUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerTrainSettleTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId);
        }

        public async Task Handle(BuildingUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId);
        }

        public async Task Handle(ExpansionSlotUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId);
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

            var expansionSlotDefault = _unitOfRepository.ExpansionSlotRepository.IsDefaultExpansionSlot(villageId);
            if (expansionSlotDefault) return;

            if (!_unitOfRepository.ExpansionSlotRepository.IsSlotAvailable(villageId)) return;

            if (_taskManager.IsExist<TrainSettlerTask>(accountId, villageId)) return;
            await _taskManager.Add<TrainSettlerTask>(accountId, villageId);
        }
    }
}