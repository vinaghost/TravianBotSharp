using MainCore.Constraints;

namespace MainCore.Commands.Features.StartHeroFarming
{
    [Handler]
    public static partial class GetTargetCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<HeroFarmTarget?> HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var accountId = command.AccountId;

            var target = context.HeroFarmTargets
                .Where(x => x.AccountId == accountId.Value)
                .OrderBy(x => x.LastSend)
                .FirstOrDefault();
            return target;
        }
    }
}