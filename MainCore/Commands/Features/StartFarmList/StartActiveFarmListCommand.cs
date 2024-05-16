using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.StartFarmList
{
    public class StartActiveFarmListCommand : FarmListCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public StartActiveFarmListCommand(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public async Task<Result> Execute(IChromeBrowser chromeBrowser, AccountId accountId)
        {
            var farmLists = GetActive(accountId);
            if (farmLists.Count == 0) return Skip.NoActiveFarmlist;

            var delayClickCommand = new DelayClickCommand();

            var html = chromeBrowser.Html;
            Result result;

            foreach (var farmList in farmLists)
            {
                var startButton = GetStartButton(html, farmList);
                if (startButton is null) return Retry.ButtonNotFound($"Start farm {farmList}");

                result = await chromeBrowser.Click(By.XPath(startButton.XPath), CancellationToken.None);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await delayClickCommand.Execute(accountId);
            }

            return Result.Ok();
        }

        private static HtmlNode GetStartButton(HtmlDocument doc, FarmId raidId)
        {
            var nodes = GetFarmNodes(doc);
            foreach (var node in nodes)
            {
                var id = GetId(node);
                if (id != raidId) continue;

                var startNode = node
                    .Descendants("button")
                    .FirstOrDefault(x => x.HasClass("startFarmList"));
                return startNode;
            }
            return null;
        }

        private List<FarmId> GetActive(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var farmListIds = context.FarmLists
                    .Where(x => x.AccountId == accountId.Value)
                    .Where(x => x.IsActive)
                    .Select(x => x.Id)
                    .AsEnumerable()
                    .Select(x => new FarmId(x))
                    .ToList();
            return farmListIds;
        }
    }
}