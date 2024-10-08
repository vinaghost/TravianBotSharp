using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.DisableContextualHelp
{
    [RegisterScoped<DisableContextualHelpCommand>]
    public class DisableContextualHelpCommand(IDataService dataService) : CommandBase(dataService), ICommand
    {
        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var option = OptionParser.GetHideContextualHelpOption(html);
            if (option is null) return Retry.NotFound("hide contextual help", "option");

            Result result;
            result = await chromeBrowser.Click(By.XPath(option.XPath), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var button = OptionParser.GetSubmitButton(html);
            if (button is null) return Retry.ButtonNotFound("submit");

            result = await chromeBrowser.Click(By.XPath(button.XPath), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}