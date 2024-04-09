using MainCore.Notification.Message;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerAlertAttack : INotificationHandler<AttackFound>
    {
        public async Task Handle(AttackFound notification, CancellationToken cancellationToken)
        {
        }
    }
}