using MainCore.Constraints;

namespace MainCore.Commands.UI.FarmingViewModel
{
    [Handler]
    public static partial class ActivationCommand
    {
        public sealed record Command(AccountId AccountId, FarmId FarmId) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            FarmListUpdated.Handler farmListUpdated,
            CancellationToken cancellationToken
            )
        {
            var (accountId, farmId) = command;

            context.FarmLists
               .Where(x => x.Id == farmId.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.IsActive, x => !x.IsActive));

            await farmListUpdated.HandleAsync(new(accountId), cancellationToken);
        }
    }
}