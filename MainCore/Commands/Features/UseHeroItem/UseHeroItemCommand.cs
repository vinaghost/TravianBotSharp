namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public static partial class UseHeroItemCommand
    {
        public sealed record Command(Dictionary<HeroItemEnums, long> ItemToUse) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ILogger logger,
            IDelayService delayService,
            CancellationToken cancellationToken)
        {
            var itemToUse = command.ItemToUse;
            Result result;
            foreach (var (item, amount) in itemToUse)
            {
                if (amount <= 0) continue;
                result = await ClickItem(browser, item, cancellationToken);
                if (result.IsFailed) return result;
                await delayService.DelayClick(cancellationToken);
                break;
            }
            foreach (var (item, amount) in itemToUse)
            {
                if (amount <= 0) continue;
                logger.Information("Use {Amount} {Item} from hero inventory", amount, item);
                result = await EnterAmount(browser, item, amount, cancellationToken);
                if (result.IsFailed) return result;
                await delayService.DelayClick(cancellationToken);
            }
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
                return InventoryParser.GetResourceTransferDialog(doc) is not null;
            }

            result = await browser.Wait(driver => loadingCompleted(driver), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private static readonly Dictionary<HeroItemEnums, string> _itemInputName = new()
            {
                { HeroItemEnums.Wood, "lumber" },
                { HeroItemEnums.Clay, "clay" },
                { HeroItemEnums.Iron, "iron" },
                { HeroItemEnums.Crop, "crop" },
            };

        private static async Task<Result> EnterAmount(
            IChromeBrowser browser,
            HeroItemEnums item,
            long amount,
            CancellationToken cancellationToken)
        {
            var (_, isFailed, element, errors) = await browser.GetElement(doc => InventoryParser.GetAmountBox(doc, _itemInputName[item]), cancellationToken);
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
                return InventoryParser.GetSuccessToast(doc) is not null;
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