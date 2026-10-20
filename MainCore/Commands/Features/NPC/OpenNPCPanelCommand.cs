using MainCore.Commands.Navigate;

namespace MainCore.Commands.Features.NPC
{
    [Handler]
    public sealed partial class OpenNPCPanelCommand(SwitchTabCommand.Handler switchTabCommand)
    {
        public sealed record Command(IChromeBrowser Browser);

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = command.Browser;
            Result result;

            result = await switchTabCommand.HandleAsync(new(0));
            if (result.IsFailed) return result;

            result = await browser.Click(NpcResourceParser.GetExchangeResourcesButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Wait(NpcResourceParser.NpcDialog(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}