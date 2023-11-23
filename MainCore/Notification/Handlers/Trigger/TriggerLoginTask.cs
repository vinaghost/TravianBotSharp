using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Services;
using MainCore.Tasks;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerLoginTask : INotificationHandler<AccountInit>
    {
        private readonly ITaskManager _taskManager;

        public TriggerLoginTask(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            Trigger(accountId);
        }

        private void Trigger(AccountId accountId)
        {
            _taskManager.AddOrUpdate<LoginTask>(accountId, first: true);
        }
    }
}