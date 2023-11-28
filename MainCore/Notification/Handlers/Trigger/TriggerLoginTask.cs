using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerLoginTask : INotificationHandler<AccountInit>, INotificationHandler<AccountLogout>
    {
        private readonly ITaskManager _taskManager;

        public TriggerLoginTask(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            await Trigger(accountId);
        }

        public async Task Handle(AccountLogout notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            await Trigger(accountId);
        }

        private async Task Trigger(AccountId accountId)
        {
            await _taskManager.AddOrUpdate<LoginTask>(accountId, first: true);
        }
    }
}