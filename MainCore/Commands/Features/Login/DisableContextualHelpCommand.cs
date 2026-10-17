using MainCore.Commands.Navigate;

namespace MainCore.Commands.Features.Login
{
    [Handler]
    public sealed partial class DisableContextualHelpCommand(ToDorfCommand.Handler toDorfCommand)
    {
        public sealed record Command(IChromeBrowser Browser);

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = command.Browser;
            Result result;

            var contextualHelpEnable = await OptionParser.IsContextualHelpEnable(browser.CurrentPage);
            if (!contextualHelpEnable) return Result.Ok();

            result = await browser.Click(OptionParser.GetOptionButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Click(OptionParser.GetHideContextualHelpOption(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Click(OptionParser.GetSubmitButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await toDorfCommand.HandleAsync(new(0));
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}