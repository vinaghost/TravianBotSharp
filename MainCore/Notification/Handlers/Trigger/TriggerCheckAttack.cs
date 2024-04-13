using MainCore.Notification.Message;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerCheckAttack : INotificationHandler<AttackFound>
    {
        private readonly ITaskManager _taskManager;

        public TriggerCheckAttack(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        public async Task Handle(AttackFound notification, CancellationToken cancellationToken)
        {
            if (_taskManager.IsExist<CheckAttackTask>(notification.AccountId, notification.VillageId)) return;
            await _taskManager.Add<CheckAttackTask>(notification.AccountId, notification.VillageId);
        }
    }
}