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
        if (result.IsFailed) return result;

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
        var errors = new List<Error>();
        var resources = await browser.CurrentPage.GetInventoryResources();
        for (var i = 0; i < 4; i++)
        {
            if (resources[i] >= requiredResources[i])
            {
                continue;
            }

            logger.Warning("Not enough resource in hero inventory. Required: {Required} {Type}, Current: {Current}", requiredResources[i], _itemInputName[i], resources[i]);
            errors.Add(new UpgradeError(UpgradeErrorType.MissingResource));
        }
        return Result.FailIfNotEmpty(errors);
    }
}