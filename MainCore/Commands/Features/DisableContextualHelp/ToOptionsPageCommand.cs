using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.DisableContextualHelp
{
    [RegisterScoped<ToOptionsPageCommand>]
    public class ToOptionsPageCommand(DataService dataService) : CommandBase(dataService), ICommand
    {
        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;

            var button = OptionParser.GetOptionButton(html);
            if (button is null) return Retry.ButtonNotFound("options");

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}