namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public sealed partial class UseHeroResourceCommand(
        IChromeBrowser browser,
        IDelayService delayService)
    {
        public sealed record Command(BuildingEnums Building, long[] Resource);

        private async ValueTask<Result> HandleAsync(
            Command command,
            CancellationToken cancellationToken)
        {
            var (building, resource) = command;

            Result result;
            var fillUpButton = await UpgradeParser.GetFillUpButton(browser.CurrentPage, building);
            result = await browser.Click(fillUpButton);
            if (result.IsFailed) return result;

            result = await browser.Wait(InventoryParser.GetResourceTransferDialog(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await IsEnoughResource(resource);
            if (result.IsFailed) return result;

            await delayService.DelayClick(cancellationToken);

            result = await browser.Click(InventoryParser.GetResourceConfirmButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Wait(InventoryParser.GetSuccessToast(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged($"&reload=auto");
            if (result.IsFailed) return result;

            await delayService.DelayClick(cancellationToken);
            return Result.Ok();
        }

        private static readonly List<string> _itemInputName = new()
            {
                "lumber",
                "clay",
                "iron",
                "crop",
            };

        private async Task<Result> IsEnoughResource(long[] requiredResources)
        {
            var errors = new List<Error>();

            for (var i = 0; i < 4; i++)
            {
                var amountInput = InventoryParser.GetAmountBox(browser.CurrentPage, _itemInputName[i]);
                var amountText = await amountInput.GetAttributeAsync("value");
                var amount = long.Parse(amountText?.Trim() ?? "0");
                if (amount < requiredResources[i])
                {
                    errors.Add(MissingResource.Error($"{_itemInputName[i]}", amount, requiredResources[i]));
                }
            }
            return Result.FailIfNotEmpty(errors);
        }
    }
}