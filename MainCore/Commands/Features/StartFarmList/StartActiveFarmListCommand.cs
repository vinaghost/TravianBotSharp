using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.StartFarmList
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class StartActiveFarmListCommand(DataService dataService, IDbContextFactory<AppDbContext> contextFactory, DelayClickCommand delayClickCommand) : CommandBase(dataService), ICommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;
        private readonly DelayClickCommand _delayClickCommand = delayClickCommand;

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;

            var farmLists = GetActive();
            if (farmLists.Count == 0) return Skip.NoActiveFarmlist;

            var html = chromeBrowser.Html;
            Result result;

            foreach (var farmList in farmLists)
            {
                var startButton = FarmListParser.GetStartButton(html, farmList);
                if (startButton is null) return Retry.ButtonNotFound($"Start farm {farmList}");

                result = await chromeBrowser.Click(By.XPath(startButton.XPath), CancellationToken.None);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await _delayClickCommand.Execute(cancellationToken);
            }

            return Result.Ok();
        }

        private List<FarmId> GetActive()
        {
            var accountId = _dataService.AccountId;
            using var context = _contextFactory.CreateDbContext();
            var farmListIds = context.FarmLists
                    .Where(x => x.AccountId == accountId.Value)
                    .Where(x => x.IsActive)
                    .Select(x => new FarmId(x.Id))
                    .ToList();
            return farmListIds;
        }
    }
}