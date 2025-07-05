using MainCore.Constraints;

namespace MainCore.Commands.UI.FarmingViewModel
{
    [Handler]
    public static partial class ActivationCommand
    {
        public sealed record Command(AccountId AccountId, FarmId FarmId) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            IRxQueue rxQueue,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var (accountId, farmId) = command;

            context.FarmLists
               .Where(x => x.Id == farmId.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.IsActive, x => !x.IsActive));

            rxQueue.Enqueue(new FarmsModified(accountId));
        }
    }
}