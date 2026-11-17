namespace MainCore.Features.UseHeroItem;

[Handler]
public sealed partial class UseHeroResourceCommand(ILogger logger)
{
    public sealed record Command(IChromeBrowser Browser, BuildingEnums Building, long[] Resource) : ICommand
    {
        public string Name => $"{nameof(UseHeroResourceCommand).Humanize()} {Description}";
        private string Description => $"{string.Join(", ", Resource.Select(x => $"{x}"))}";
    }

    private async ValueTask<Result> HandleAsync(Command command)
    {
        var (browser, building, resource) = command;

        Result result;
        var fillUpButton = await browser.CurrentPage.GetFillUpButton(building);
        result = await browser.Click(fillUpButton);
        if (result.IsFailed) return result;

        result = await browser.Wait(browser.CurrentPage.GetResourceTransferDialog());
        if (result.IsFailed)
        {
            result = await browser.Click(fillUpButton);
            if (result.IsFailed) return result;
            
            result = await browser.Wait(browser.CurrentPage.GetResourceTransferDialog());
            if (result.IsFailed) return result;
        }

        result = await IsEnoughResource(browser, resource);
        if (result.IsFailed)
        {
            await browser.Refresh();
            return result;
        }

        result = await browser.Click(browser.CurrentPage.GetResourceConfirmButton());
        if (result.IsFailed) return result;

        result = await browser.WaitPageChanged("&reload=auto");
        if (result.IsFailed) return result;

        result = await browser.WaitPageChanged(@"^(?!.*reload=auto).*");
        if (result.IsFailed) return result;

        return Result.Ok();
    }

    private static readonly List<string> _itemInputName =
    [
        "lumber",
        "clay",
        "iron",
        "crop",
    ];

    private async Task<Result> IsEnoughResource(IChromeBrowser browser, long[] requiredResources)
    {
        var resources = await browser.CurrentPage.GetInventoryResources();

        var missingResources = Enumerable.Range(0, 4)
            .Where(i => resources[i] < requiredResources[i]);

        foreach (int i in missingResources)
        {
            logger.Warning(
                "Not enough resource in hero inventory. Required: {Required} {Type}, Current: {Current}",
                requiredResources[i],
                _itemInputName[i],
                resources[i]
            );
        }

        return Result.FailIf(missingResources.Any(), new UpgradeError(UpgradeErrorType.MissingResource));
    }
}