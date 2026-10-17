using MainCore.Commands.Features.ClaimVillageQuest;
using MainCore.Infrasturecture.Extensions;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class ClaimVillageQuestTask(
        IChromeBrowser browser,
        OpenVillageQuestPageCommand.Handler openVillageQuestPageCommand,
        ClaimRewardCommand.Handler claimRewardCommand)
    {
        public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
        {
            protected override string TaskName => "Claim village quest";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(VillageId, VillageSettingEnums.AutoClaimQuestEnable);
                if (!settingEnable) return false;

                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(
#pragma warning disable S1172 // Unused method parameters should be removed
#pragma warning disable IDE0060 // Remove unused parameter
            Task task,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore S1172 // Unused method parameters should be removed
            CancellationToken cancellationToken)
        {
            Result result;
            var canClaimQuest = await QuestParser.IsQuestClaimable(browser.CurrentPage);
            if (!canClaimQuest) return Result.Ok();

            result = await openVillageQuestPageCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;
            result = await claimRewardCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}