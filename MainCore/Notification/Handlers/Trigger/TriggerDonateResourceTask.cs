using MainCore.Common.Enums;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerDonateResourceTask : INotificationHandler<AccountSettingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerDonateResourceTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(AccountSettingUpdated notification, CancellationToken cancellationToken)
        {
            var enableDonateResource = _unitOfRepository.AccountSettingRepository.GetBooleanByName(notification.AccountId, AccountSettingEnums.EnableDonateResource);

            if (enableDonateResource) return;
            var villages = _unitOfRepository.VillageRepository.Get(notification.AccountId);
            foreach (var village in villages)
            {
                var task = _taskManager.Get<DonateResourceTask>(notification.AccountId, village);
                await _taskManager.Remove(notification.AccountId, task);
            }
        }
    }
}