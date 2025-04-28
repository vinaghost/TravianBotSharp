using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.StartFarmList
{
    [RegisterScoped<StartAllFarmListCommand>]
    public class StartAllFarmListCommand(IDataService dataService) : CommandBase(dataService), ICommand
    {
        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var startAllButton = FarmListParser.GetStartAllButton(html);
            if (startAllButton is null) return Retry.ButtonNotFound("Start all farms");

            Result result;
            result = await chromeBrowser.Click(By.XPath(startAllButton.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}