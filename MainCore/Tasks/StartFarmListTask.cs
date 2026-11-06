using MainCore.Commands.Features.StartFarmList;
using MainCore.Commands.Navigate;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class StartFarmListTask(
        IDbContextFactory<AppDbContext> contextFactory,
        ChromeManager chromeManager,
        ToFarmListPageCommand.Handler toFarmListPageCommand,
        StartActiveFarmlistCommand.Handler startActiveFarmlistCommand,
        StartAllFarmlistCommand.Handler startAllFarmlistCommand
        )
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => nameof(StartFarmListTask).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            var browser = chromeManager.Get(task.AccountId);
            Result result;
            result = await toFarmListPageCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            if (UseStartAllButton(task.AccountId))
            {
                result = await startAllFarmlistCommand.HandleAsync(new(browser), cancellationToken);
                if (result.IsFailed) return result;
            }
            else
            {
                var farmLists = GetActiveFarms(task.AccountId);
                if (farmLists.Count == 0) return Skip.Error.WithError("No farmlist is active");

                result = await startActiveFarmlistCommand.HandleAsync(new(browser, farmLists), cancellationToken);
                if (result.IsFailed) return result;
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
    }
}