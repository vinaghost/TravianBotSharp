using MainCore.Constraints;

namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public static partial class ToHeroInventoryCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {

            var html = browser.Html;

            var avatar = InventoryParser.GetHeroAvatar(html);
            if (avatar is null) return Retry.ButtonNotFound("avatar hero");

            static bool TabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryPage(doc);
            }

            var result = await browser.Click(By.XPath(avatar.XPath));
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("hero", TabActived, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}