using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerStartAdventureTask : INotificationHandler<AdventureUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerStartAdventureTask(ITaskManager taskManager, UnitOfRepository unitOfRepository)
        {
            _taskManager = taskManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(AdventureUpdated notification, CancellationToken cancellationToken)
        {
            await Trigger(notification.AccountId);
        }

        private async Task Trigger(AccountId accountId)
        {
            var autoStartAdventure = _unitOfRepository.AccountSettingRepository.GetBooleanByName(accountId, AccountSettingEnums.AutoStartAdventure);
            if (autoStartAdventure)
            {
                if (_taskManager.IsExist<StartAdventureTask>(accountId)) return;
                await _taskManager.Add<StartAdventureTask>(accountId);
            }
            else
            {
                var task = _taskManager.Get<StartAdventureTask>(accountId);
                await _taskManager.Remove(accountId, task);
            }
        }
    }
}