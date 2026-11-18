using MainCore.Features.Navigate;

namespace MainCore.Features.Login;

[Handler]
public sealed partial class DisableContextualHelpCommand(ToDorfCommand.Handler toDorfCommand)
{
    public sealed record Command(IChromeBrowser Browser) : ICommand
    {
        public string Name => nameof(DisableContextualHelpCommand).Humanize();
    }

    private async ValueTask<Result> HandleAsync(Command command)
    {
        var browser = command.Browser;
        Result result;

        var contextualHelp = browser.CurrentPage.GetContextualHelp();
        if (await contextualHelp.CountAsync() == 0)
        {
            return Result.Ok();
        }

        result = await browser.Click(browser.CurrentPage.GetOptionButton());
        if (result.IsFailed) return result;

        result = await browser.Click(browser.CurrentPage.GetHideContextualHelpOption());
        if (result.IsFailed) return result;

        result = await browser.Click(browser.CurrentPage.GetSubmitButton());
        if (result.IsFailed) return result;

        result = await toDorfCommand.HandleAsync(new(command.Browser, 0));
        if (result.IsFailed) return result;
        return Result.Ok();
    }
}