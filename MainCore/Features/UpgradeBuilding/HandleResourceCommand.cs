using MainCore.Features.Update;
using MainCore.Features.UseHeroItem;

namespace MainCore.Features.UpgradeBuilding;

[Handler]
public sealed partial class HandleResourceCommand(
    UseHeroResourceCommand.Handler useHeroResourceCommand,
    ILogger logger)
{
    public sealed record Command(IChromeBrowser Browser, NormalBuildPlan Plan, bool CanUseHeroResource) : ICommand
    {
        public string Name => $"{nameof(HandleResourceCommand).Humanize()} [{Description}]";
        private string Description => $"{Plan}";
    }

    private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var (browser, plan, canUseHeroResource) = command;

        var storage = await UpdateStorageCommand.GetData(browser.CurrentPage);
        var requiredResource = await GetRequiredResource(browser.CurrentPage, plan.Type);

        var (_, isFailed, errors) = IsEnoughResource(storage, requiredResource);
        if (!isFailed) return Result.Ok();

        if (errors.Any(x => x is UpgradeError { Type: UpgradeErrorType.StorageLimit or UpgradeErrorType.LackOfFreeCrop }))
        {
            return Result.Fail(errors);
        }

        if (!canUseHeroResource) return Result.Fail(errors);

        logger.Information("Use resource in hero invetory to upgrade building");
        var missingResource = GetMissingResource(storage, requiredResource);

        var result = await useHeroResourceCommand.HandleAsync(new(browser, plan.Type, missingResource), cancellationToken);
        if (result.IsFailed) return result;

        return Result.Ok();
    }

    private static async Task<long[]> GetRequiredResource(IPage page, BuildingEnums building)
    {
        var resources = await page.GetRequiredResource(building);
        var count = await resources.CountAsync();

        if (count != 5) throw new InvalidOperationException($"Expected 5 elements, but found {count} when fetching required resources.");

        var resourceBuilding = new long[5];
        for (var i = 0; i < count; i++)
        {
            var resource = resources.Nth(i);
            resourceBuilding[i] = (await resource.InnerTextAsync()).ParseLong();
        }

        return resourceBuilding;
    }

    private static long[] GetMissingResource(StorageDto storage, long[] requiredResource)
    {
        var resource = new long[4];
        if (storage.Wood < requiredResource[0]) resource[0] = requiredResource[0] - storage.Wood;
        if (storage.Clay < requiredResource[1]) resource[1] = requiredResource[1] - storage.Clay;
        if (storage.Iron < requiredResource[2]) resource[2] = requiredResource[2] - storage.Iron;
        if (storage.Crop < requiredResource[3]) resource[3] = requiredResource[3] - storage.Crop;
        return resource;
    }

    private Result IsEnoughResource(StorageDto storage, long[] resource)
    {
        var errors = new List<Error>();
        if (storage.Wood < resource[0])
        {
            logger.Warning("Don't have enough wood, need {Required} but have {Storage} ({Difference})", resource[0], storage.Wood, resource[0] - storage.Wood);
            errors.Add(new UpgradeError(UpgradeErrorType.MissingResource));
        }

        if (storage.Clay < resource[1])
        {
            logger.Warning("Don't have enough clay, need {Required} but have {Storage} ({Difference})", resource[1], storage.Clay, resource[1] - storage.Clay);
            errors.Add(new UpgradeError(UpgradeErrorType.MissingResource));
        }

        if (storage.Iron < resource[2])
        {
            logger.Warning("Don't have enough iron, need {Required} but have {Storage} ({Difference})", resource[2], storage.Iron, resource[2] - storage.Iron);
            errors.Add(new UpgradeError(UpgradeErrorType.MissingResource));
        }

        if (storage.Crop < resource[3])
        {
            logger.Warning("Don't have enough crop, need {Required} but have {Storage} ({Difference})", resource[3], storage.Crop, resource[3] - storage.Crop);
            errors.Add(new UpgradeError(UpgradeErrorType.MissingResource));
        }

        if (resource.Length == 5 && storage.FreeCrop < resource[4])
        {
            logger.Warning("Don't have enough free crop, need {Required} but have {Storage} ({Difference})", resource[4], storage.FreeCrop, resource[4] - storage.FreeCrop);
            errors.Add(new UpgradeError(UpgradeErrorType.LackOfFreeCrop));
        }

        if (storage.Granary < resource[3])
        {
            logger.Warning("Granary limit reached, need {Required} but have {Storage} ({Difference})", resource[3], storage.Granary, resource[3] - storage.Granary);
            errors.Add(new UpgradeError(UpgradeErrorType.StorageLimit));
        }

        var max = resource.Take(3).Max();
        if (storage.Warehouse < max)
        {
            logger.Warning("Warehouse limit reached, need {Required} but have {Storage} ({Difference})", max, storage.Warehouse, max - storage.Warehouse);
            errors.Add(new UpgradeError(UpgradeErrorType.StorageLimit));
        }

        return Result.FailIfNotEmpty(errors);
    }
}