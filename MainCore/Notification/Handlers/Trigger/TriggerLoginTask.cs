using Immediate.Handlers.Shared;
using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class LoginTrigger
    {
        private static async ValueTask HandleAsync(
            AccountInit command,
            ITaskManager taskManager,
            CancellationToken cancellationToken
        )
        {
            await taskManager.AddOrUpdate<LoginTask>(command.AccountId, first: true);
        }
    }

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