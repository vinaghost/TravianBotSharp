namespace MainCore.Features.ClaimVillageQuest;

[Handler]
public sealed partial class ClaimVillageQuestTask(
    ChromeManager chromeManager,
    OpenVillageQuestPageCommand.Handler openVillageQuestPageCommand,
    ClaimRewardCommand.Handler claimRewardCommand)
{
    public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
    {
        protected override string TaskName => nameof(ClaimVillageQuestTask).Humanize();

        public override bool CanStart(AppDbContext context)
        {
            var settingEnable = context.BooleanByName(VillageId, VillageSettingEnums.AutoClaimQuestEnable);
            if (!settingEnable) return false;

            return true;
        }
    }

    private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
    {
        var browser = chromeManager.Get(task.AccountId);

        var canClaimQuest = await browser.CurrentPage.IsVillageQuestClaimable();
        if (!canClaimQuest) return Result.Ok();

        var result = await openVillageQuestPageCommand.HandleAsync(new(browser), cancellationToken);
        if (result.IsFailed) return result;

        result = await claimRewardCommand.HandleAsync(new(browser), cancellationToken);
        if (result.IsFailed) return result;

        return Result.Ok();
    }
}