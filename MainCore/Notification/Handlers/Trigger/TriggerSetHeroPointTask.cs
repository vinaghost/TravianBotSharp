using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerSetHeroPointTask : INotificationHandler<AccountInit>, INotificationHandler<HeroLevelUpdated>, INotificationHandler<AccountSettingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerSetHeroPointTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(HeroLevelUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId);
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId);
        }

        public async Task Handle(AccountSettingUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId);
        }

        private async Task Trigger(AccountId accountId)
        {
            var autoSetHeroPoint = _unitOfRepository.AccountSettingRepository.GetBooleanByName(accountId, AccountSettingEnums.EnableAutoSetHeroPoint);
            if (autoSetHeroPoint)
            {
                if (_taskManager.IsExist<SetHeroPointTask>(accountId)) return;
                await _taskManager.Add<SetHeroPointTask>(accountId);
            }
            else
            {
                var task = _taskManager.Get<SetHeroPointTask>(accountId);
                await _taskManager.Remove(accountId, task);
            }
        }
    }
}