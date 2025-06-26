using MainCore.Constraints;

namespace MainCore.Commands.Features.StartHeroFarming
{
    [Handler]
    public static partial class ValidateHeroHeathCommand
    {
        public sealed record Command(AccountId AccountId, int heroHealth) : IAccountCommand;

        private static async ValueTask<bool> HandleAsync(
            Command command,
            ILogger logger,
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var (accountId, heroHealth) = command;

            var heroHealthCondition = context.AccountsSetting
                .Where(x => x.AccountId == accountId.Value && x.Setting == AccountSettingEnums.HeroFarmingHealthCondition)
                .Select(x => x.Value)
                .FirstOrDefault();

            if (heroHealth < heroHealthCondition)
            {
                logger.Warning("Hero health is below the required condition ({HeroHealthCondition}), cannot proceed.", heroHealthCondition);
                return false;
            }

            return true;
        }
    }
}