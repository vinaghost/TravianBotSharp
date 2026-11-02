using MainCore.Commands.Navigate;

namespace MainCore.Commands.Features.NPC
{
    [Handler]
    public sealed partial class OpenNPCPanelCommand(
        SwitchTabCommand.Handler switchTabCommand,
        ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => nameof(OpenNPCPanelCommand);
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            Result result;

            result = await switchTabCommand.HandleAsync(new(command.AccountId, 0));
            if (result.IsFailed) return result;

            result = await browser.Click(NpcResourceParser.GetExchangeResourcesButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Wait(NpcResourceParser.NpcDialog(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}