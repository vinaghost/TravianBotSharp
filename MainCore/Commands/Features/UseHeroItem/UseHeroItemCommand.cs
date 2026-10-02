namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public sealed partial class UseHeroItemCommand(IChromeBrowser browser, ILogger logger)
    {
        public sealed record Command(Dictionary<HeroItemEnums, long> ItemToUse) : ICommand;

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var itemToUse = command.ItemToUse;
            Result result;
            foreach (var (item, amount) in itemToUse)
            {
                if (amount <= 0) continue;
                result = await ClickItem(browser, item);
                if (result.IsFailed) return result;
                break;
            }
            foreach (var (item, amount) in itemToUse)
            {
                if (amount <= 0) continue;
                logger.Information("Use {Amount} {Item} from hero inventory", amount, item);
                result = await EnterAmount(browser, item, amount);
                if (result.IsFailed) return result;
            }
            result = await Confirm(browser);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async Task<Result> ClickItem(
            IChromeBrowser browser,
            HeroItemEnums item)
        {
            Result result;
            result = await browser.Click(InventoryParser.GetItemSlot(browser.CurrentPage, item));
            if (result.IsFailed) return result;

            result = await browser.Wait(InventoryParser.GetResourceTransferDialog(browser.CurrentPage));
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
            long amount)
        {
            Result result;
            result = await browser.Input(InventoryParser.GetAmountBox(browser.CurrentPage, _itemInputName[item]), amount.ToString());
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private static async Task<Result> Confirm(IChromeBrowser browser)
        {
            Result result;
            result = await browser.Click(InventoryParser.GetConfirmButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Wait(InventoryParser.GetSuccessToast(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}