using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class ClaimQuestTask(
        IChromeBrowser browser,
        IDelayService delayService,
        SwitchTabCommand.Handler switchTabCommand)
    {
        public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
        {
            protected override string TaskName => "Claim quest";

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
            result = await ToQuestPage();
            if (result.IsFailed) return result;
            result = await ClaimQuest(cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async ValueTask<Result> ToQuestPage()
        {
            var questMaster = QuestParser.GetQuestMaster(browser.CurrentPage);

            var result = await browser.Click(questMaster);
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("tasks");
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async ValueTask<Result> ClaimQuest(CancellationToken cancellationToken)
        {
            Result result;

            do
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Cancel.Error;
                }
                var quest = QuestParser.GetQuestCollectButton(browser.CurrentPage);

                if (await quest.CountAsync() == 0)
                {
                    result = await switchTabCommand.HandleAsync(new(1), cancellationToken);
                    if (result.IsFailed) return result;

                    await delayService.DelayClick(cancellationToken);

                    quest = QuestParser.GetQuestCollectButton(browser.CurrentPage);
                    result = await browser.Click(quest);
                    if (result.IsFailed) return result;
                    continue;
                }
                else
                {
                    result = await browser.Click(quest);
                    if (result.IsFailed) return result;
                    await delayService.DelayClick(cancellationToken);
                }
            }
            while (await QuestParser.IsQuestClaimable(browser.CurrentPage));

            return Result.Ok();
        }
    }
}