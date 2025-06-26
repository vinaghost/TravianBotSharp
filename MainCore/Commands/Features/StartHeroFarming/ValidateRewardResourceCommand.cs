using MainCore.Constraints;

namespace MainCore.Commands.Features.StartHeroFarming
{
    [Handler]
    public static partial class ValidateRewardResourceCommand
    {
        public sealed record Command(AccountId AccountId, int rewardResource) : IAccountCommand;

        private static async ValueTask<bool> HandleAsync(
            Command command,
            ILogger logger,
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var (accountId, rewardResource) = command;

            var rewardResourceCondition = context.AccountsSetting
                .Where(x => x.AccountId == accountId.Value && x.Setting == AccountSettingEnums.HeroFarmingHealthCondition)
                .Select(x => x.Value)
                .FirstOrDefault();

            if (rewardResource < rewardResourceCondition)
            {
                logger.Warning("Reward resource after battle is below the condition ({RewardResourceCondition}), cannot start hero farming.", rewardResourceCondition);
                return false;
            }

            return true;
        }
    }
}