#pragma warning disable S1172

namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public static partial class ToHeroInventoryCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            IDelayService delayService,
            CancellationToken cancellationToken)
        {
            var result = await browser.Click(InventoryParser.GetHeroAvatar(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Wait(InventoryParser.GetInventoryPageWrapper(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Wait(InventoryParser.GetInventoryPageWrapper(browser.CurrentPage), condition: "node => node.classList.contains('loading')");
            if (result.IsFailed) return result;

            await delayService.DelayTask(cancellationToken);

            return Result.Ok();
        }
    }
}