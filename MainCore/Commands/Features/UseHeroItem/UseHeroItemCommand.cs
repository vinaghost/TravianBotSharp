namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public static partial class UseHeroItemCommand
    {
        public sealed record Command(HeroItemEnums Item, long Amount) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ILogger logger,
            IDelayService delayService,
            CancellationToken cancellationToken)
        {
            var (item, amount) = command;
            logger.Information("Use {Amount} {Item} from hero inventory", amount, item);

            var result = await ClickItem(browser, item, cancellationToken);
            if (result.IsFailed) return result;
            await delayService.DelayClick(cancellationToken);

            result = await EnterAmount(browser, amount);
            if (result.IsFailed) return result;
            await delayService.DelayClick(cancellationToken);

            result = await Confirm(browser, cancellationToken);
            if (result.IsFailed) return result;
            await delayService.DelayClick(cancellationToken);

            return Result.Ok();
        }

        private static async Task<Result> ClickItem(
            IChromeBrowser browser,
            HeroItemEnums item,
            CancellationToken cancellationToken)
        {
            var node = InventoryParser.GetItemSlot(browser.Html, item);
            if (node is null) return Retry.NotFound($"{item}", "item");

            static bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryLoaded(doc);
            }

            Result result;
            result = await browser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result;

            result = await browser.Wait(driver => loadingCompleted(driver), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private static async Task<Result> EnterAmount(
            IChromeBrowser browser,
            long amount
            )
        {
            var node = InventoryParser.GetAmountBox(browser.Html);
            if (node is null) return Retry.TextboxNotFound("amount");

            Result result;
            result = await browser.Input(By.XPath(node.XPath), amount.ToString());
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private static async Task<Result> Confirm(
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var node = InventoryParser.GetConfirmButton(browser.Html);
            if (node is null) return Retry.ButtonNotFound("confirm");

            static bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryLoaded(doc);
            }

            Result result;
            result = await browser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result;

            result = await browser.Wait(driver => loadingCompleted(driver), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}