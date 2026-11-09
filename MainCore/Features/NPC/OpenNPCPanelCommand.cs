using MainCore.Features.Navigate;

namespace MainCore.Features.NPC;

[Handler]
public sealed partial class OpenNPCPanelCommand(SwitchTabCommand.Handler switchTabCommand)
{
    public sealed record Command(IChromeBrowser Browser) : ICommand
    {
        public string Name => nameof(OpenNPCPanelCommand).Humanize();
    }

    private async ValueTask<Result> HandleAsync(Command command)
    {
        var browser = command.Browser;
        Result result;

        result = await switchTabCommand.HandleAsync(new(browser, 0));
        if (result.IsFailed) return result;

        result = await browser.Click(browser.CurrentPage.GetExchangeResourcesButton());
        if (result.IsFailed) return result;

        result = await browser.Wait(browser.CurrentPage.NpcDialog());
        if (result.IsFailed) return result;

        return Result.Ok();
    }
}