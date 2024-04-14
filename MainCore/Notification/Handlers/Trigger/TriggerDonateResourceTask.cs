using MainCore.Common.Enums;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerDonateResourceTask : INotificationHandler<VillageSettingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerDonateResourceTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            var enableDonateResource = _unitOfRepository.VillageSettingRepository.GetBooleanByName(notification.VillageId, VillageSettingEnums.EnableDonateResource);
            if (enableDonateResource) return;

            var task = _taskManager.Get<DonateResourceTask>(notification.AccountId, notification.VillageId);
            await _taskManager.Remove(notification.AccountId, task);
        }
    }
}