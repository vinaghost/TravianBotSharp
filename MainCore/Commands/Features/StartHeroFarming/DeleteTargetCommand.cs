using MainCore.Constraints;

namespace MainCore.Commands.Features.StartHeroFarming
{
    [Handler]
    public static partial class DeleteTargetCommand
    {
        public sealed record Command(AccountId AccountId, HeroFarmTarget Target) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var (accountId, target) = command;
            context.Remove(target);
            context.SaveChanges();
        }
    }
}