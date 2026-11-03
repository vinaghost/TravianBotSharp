#pragma warning disable S1172

namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public partial class ToHeroInventoryCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => nameof(ToHeroInventoryCommand).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            var result = await browser.Click(InventoryParser.GetHeroAvatar(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Wait(InventoryParser.GetInventoryPageWrapper(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Wait(InventoryParser.GetInventoryPageWrapper(browser.CurrentPage), condition: "node => node.classList.contains('loading')");
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}