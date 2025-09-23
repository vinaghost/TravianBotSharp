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
            CancellationToken cancellationToken)
        {
            var (_, isFailed, element, errors) = await browser.GetElement(doc => InventoryParser.GetHeroAvatar(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            var result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            static bool TabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryPage(doc);
            }

            result = await browser.Wait(TabActived, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}