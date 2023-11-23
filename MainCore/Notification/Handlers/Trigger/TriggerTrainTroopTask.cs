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
        private readonly IUnitOfRepository _unitOfRepository;

        public TriggerTrainTroopTask(ITaskManager taskManager, IUnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            Trigger(accountId, villageId);
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;

            var villages = _unitOfRepository.VillageRepository.Get(accountId);
            foreach (var village in villages)
            {
                Trigger(accountId, village);
            }
        }

        private void Trigger(AccountId accountId, VillageId villageId)
        {
            var trainTroopEnable = _unitOfRepository.VillageSettingRepository.GetBooleanByName(villageId, VillageSettingEnums.TrainTroopEnable);
            if (!trainTroopEnable) return;

            if (_taskManager.IsExist<TrainTroopTask>(accountId, villageId)) return;
            _taskManager.Add<TrainTroopTask>(accountId, villageId);
        }
    }
}