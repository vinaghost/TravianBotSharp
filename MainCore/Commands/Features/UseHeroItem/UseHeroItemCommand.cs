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

            result = await EnterAmount(browser, amount, cancellationToken);
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
            var (_, isFailed, element, errors) = await browser.GetElement(doc => InventoryParser.GetItemSlot(doc, item), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            Result result;
            result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            static bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryLoaded(doc);
            }

            result = await browser.Wait(driver => loadingCompleted(driver), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private static async Task<Result> EnterAmount(
            IChromeBrowser browser,
            long amount,
            CancellationToken cancellationToken)
        {
            var (_, isFailed, element, errors) = await browser.GetElement(doc => InventoryParser.GetAmountBox(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            Result result;
            result = await browser.Input(element, amount.ToString(), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private static async Task<Result> Confirm(
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var (_, isFailed, element, errors) = await browser.GetElement(doc => InventoryParser.GetConfirmButton(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            static bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryLoaded(doc);
            }

            Result result;
            result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            result = await browser.Wait(driver => loadingCompleted(driver), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}