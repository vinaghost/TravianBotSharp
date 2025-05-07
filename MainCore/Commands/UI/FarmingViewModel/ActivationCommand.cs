using MainCore.Constraints;
using MainCore.Notification.Behaviors;

namespace MainCore.Commands.UI.FarmingViewModel
{
    [Handler]
    [Behaviors(typeof(FarmListUpdatedBehavior<,>))]
    public static partial class ActivationCommand
    {
        public sealed record Command(AccountId AccountId, FarmId FarmId) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var (accountId, farmId) = command;

            context.FarmLists
               .Where(x => x.Id == farmId.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.IsActive, x => !x.IsActive));
        }
    }
}