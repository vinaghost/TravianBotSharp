using MainCore.Commands.Base;

namespace MainCore.Commands.UI.FarmingViewModel
{
    [Handler]
    public static partial class ActivationCommand
    {
        public sealed record Command(AccountId AccountId, FarmId FarmId) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            IDbContextFactory<AppDbContext> contextFactory,
            FarmListUpdated.Handler farmListUpdated,
            CancellationToken cancellationToken
            )
        {
            var (accountId, farmId) = command;
            using var context = await contextFactory.CreateDbContextAsync();
            context.FarmLists
               .Where(x => x.Id == farmId.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.IsActive, x => !x.IsActive));

            await farmListUpdated.HandleAsync(new(accountId), cancellationToken);
        }
    }
}