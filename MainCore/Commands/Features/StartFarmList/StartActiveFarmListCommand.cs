namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class StartActiveFarmListCommand
    {
        public sealed record Command(AccountId AccountId) : ICustomCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            IDbContextFactory<AppDbContext> contextFactory,
            DelayClickCommand.Handler delayClickCommand,
            CancellationToken cancellationToken)
        {
            var chromeBrowser = chromeManager.Get(command.AccountId);

            var farmLists = GetActive(command.AccountId, contextFactory);
            if (farmLists.Count == 0) return Skip.NoActiveFarmlist;

            var html = chromeBrowser.Html;

            foreach (var farmList in farmLists)
            {
                var startButton = FarmListParser.GetStartButton(html, farmList);
                if (startButton is null) return Retry.ButtonNotFound($"Start farm {farmList}");

                var result = await chromeBrowser.Click(By.XPath(startButton.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await delayClickCommand.HandleAsync(new(command.AccountId), cancellationToken);
            }

            return Result.Ok();
        }

        private static List<FarmId> GetActive(AccountId accountId, IDbContextFactory<AppDbContext> contextFactory)
        {
            using var context = contextFactory.CreateDbContext();
            return context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.IsActive)
                .Select(x => new FarmId(x.Id))
                .ToList();
        }
    }
}