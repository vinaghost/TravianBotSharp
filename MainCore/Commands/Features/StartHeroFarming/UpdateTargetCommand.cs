using MainCore.Constraints;

namespace MainCore.Commands.Features.StartHeroFarming
{
    [Handler]
    public static partial class UpdateTargetCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            context.SaveChanges();
        }
    }
}