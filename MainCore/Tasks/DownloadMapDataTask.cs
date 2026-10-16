using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class DownloadMapDataTask(
        IDbContextFactory<AppDbContext> contextFactory,
        MapDataService mapDataService)
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Download map data";
        }

        private async ValueTask<Result> HandleAsync(
            Task task,
            CancellationToken cancellationToken)
        {
            // map.sql changes once a day; checking every few hours makes sure each day gets a snapshot
            task.ExecuteAt = DateTime.Now.AddHours(6);

            using var context = contextFactory.CreateDbContext();
            var server = context.Accounts
                .Where(x => x.Id == task.AccountId.Value)
                .Select(x => x.Server)
                .First();

            var result = await mapDataService.DownloadToday(server, cancellationToken);
            if (result.IsFailed) return Skip.Error.WithErrors(result.Errors);
            return Result.Ok();
        }
    }
}
