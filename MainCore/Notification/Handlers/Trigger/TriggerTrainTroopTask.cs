using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerTrainTroopTask : INotificationHandler<VillageSettingUpdated>, INotificationHandler<AccountInit>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerTrainTroopTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId, notification.VillageId);
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            var villages = _unitOfRepository.VillageRepository.Get(notification.AccountId);
            foreach (var village in villages)
            {
                await Trigger(notification.AccountId, village);
            }
        }

        private async Task Trigger(AccountId accountId, VillageId villageId)
        {
            var trainTroopEnable = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.TrainTroopEnable);
            if (trainTroopEnable)
            {
                if (_taskManager.IsExist<TrainTroopTask>(accountId, villageId)) return;
                await _taskManager.Add<TrainTroopTask>(accountId, villageId);
            }
            else
            {
                var task = _taskManager.Get<TrainTroopTask>(accountId, villageId);
                await _taskManager.Remove(accountId, task);
            }
        }
    }
}