using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class StartFarmListTask(
        IDbContextFactory<AppDbContext> contextFactory,
        IChromeBrowser browser,
        IDelayService delayService,
        ToFarmListPageCommand.Handler toFarmListPageCommand
        )
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Start farm list";
        }

        private async ValueTask<Result> HandleAsync(
            Task task,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toFarmListPageCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            if (UseStartAllButton(task.AccountId))
            {
                result = await browser.Click(FarmListParser.GetStartAllButton(browser.CurrentPage));
                if (result.IsFailed) return result;
            }
            else
            {
                var farmLists = GetActiveFarms(task.AccountId);
                if (farmLists.Count == 0) return Skip.Error.WithError("No farmlist is active");

                foreach (var farmList in farmLists)
                {
                    result = await browser.Click(FarmListParser.GetStartButton(browser.CurrentPage, farmList));
                    if (result.IsFailed) return result;

                    await delayService.DelayClick(cancellationToken);
                }
            }

            task.ExecuteAt = GetNextExecuteTime(task.AccountId);
            return Result.Ok();
        }

        private bool UseStartAllButton(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            var useStartAllButton = context.BooleanByName(accountId, AccountSettingEnums.UseStartAllButton);
            return useStartAllButton;
        }

        private DateTime GetNextExecuteTime(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();

            var seconds = context.ByName(
                accountId,
                AccountSettingEnums.FarmIntervalMin,
                AccountSettingEnums.FarmIntervalMax);
            var nextExecute = DateTime.Now.AddSeconds(seconds);
            return nextExecute;
        }

        private List<FarmId> GetActiveFarms(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            var farmLists = context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.IsActive)
                .Select(x => new FarmId(x.Id))
                .ToList();
            return farmLists;
        }
    }
}