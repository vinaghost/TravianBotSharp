using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerReviveHeroTask : INotificationHandler<AccountInit>, INotificationHandler<HeroDead>, INotificationHandler<AccountSettingUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerReviveHeroTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(HeroDead notification, CancellationToken cancellationToken)
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
            var autoReviveHero = _unitOfRepository.AccountSettingRepository.GetBooleanByName(accountId, AccountSettingEnums.EnableAutoReviveHero);
            if (autoReviveHero)
            {
                if (_taskManager.IsExist<ReviveHeroTask>(accountId)) return;
                await _taskManager.Add<ReviveHeroTask>(accountId);
            }
            else
            {
                var task = _taskManager.Get<ReviveHeroTask>(accountId);
                await _taskManager.Remove(accountId, task);
            }
        }
    }
}