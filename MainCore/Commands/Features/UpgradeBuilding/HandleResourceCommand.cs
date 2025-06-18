using MainCore.Commands.Features.UseHeroItem;
using MainCore.Constraints;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class HandleResourceCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, NormalBuildPlan Plan) : IAccountVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            JobUpdated.Handler jobUpdated,
            UpdateStorageCommand.Handler updateStorageCommand,
            UseHeroResourceCommand.Handler useHeroResourceCommand,
            ToHeroInventoryCommand.Handler toHeroInventoryCommand,
            UpdateInventoryCommand.Handler updateInventoryCommand,
            GetLowestBuildingQuery.Handler getLowestBuildingQuery,
            GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery, // <-- Injected
            AddJobCommand.Handler addJobCommand,
            ISettingService settingService,
            IChromeBrowser browser,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId, plan) = command;

            var storage = await updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            var requiredResource = GetRequiredResource(browser, plan.Type);

            Result result = storage.IsResourceEnough(requiredResource);
            if (!result.IsFailed) return Result.Ok();

            if (result.HasError<LackOfFreeCrop>(out var freeCropErrors))
            {
                var message = string.Join(Environment.NewLine, freeCropErrors.Select(x => x.Message));
                logger.Warning("{Error}", message);

                var cropland = await getLowestBuildingQuery.HandleAsync(new(villageId, BuildingEnums.Cropland), cancellationToken);

                var cropLandPlan = new NormalBuildPlan()
                {
                    Location = cropland.Location,
                    Type = cropland.Type,
                    Level = cropland.Level + 1,
                };

                await addJobCommand.HandleAsync(new(villageId, cropLandPlan.ToJob(), true), cancellationToken);
                await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
                return Continue.Error;
            }

            if (result.HasError<StorageLimit>(out var storageLimitErrors))
            {
                var message = string.Join(Environment.NewLine, storageLimitErrors.Select(x => x.Message));
                logger.Warning("{Error}", message);

                // Determine if it's a warehouse or granary issue
                var warehouseError = storageLimitErrors.OfType<MainCore.Errors.StorageLimit>().FirstOrDefault(e => e.Message.Contains("Warehouse"));
                var granaryError = storageLimitErrors.OfType<MainCore.Errors.StorageLimit>().FirstOrDefault(e => e.Message.Contains("Granary"));

                if (warehouseError != null)
                {
                    // Find the lowest level warehouse
                    var warehouse = await getLowestBuildingQuery.HandleAsync(new(villageId, BuildingEnums.Warehouse), cancellationToken);
                    var layoutBuildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId), cancellationToken);
                    var warehouseBuilding = layoutBuildings.FirstOrDefault(x => x.Type == BuildingEnums.Warehouse && x.Location == warehouse.Location);
                    if (warehouseBuilding != null && (warehouseBuilding.QueueLevel > warehouseBuilding.CurrentLevel || warehouseBuilding.JobLevel > warehouseBuilding.CurrentLevel))
                    {
                        // Wait for upgrade to finish
                        return Skip.WaitingStorageUpgrade;
                    }
                    // Queue warehouse upgrade
                    var warehousePlan = new NormalBuildPlan()
                    {
                        Location = warehouse.Location,
                        Type = warehouse.Type,
                        Level = warehouse.Level + 1,
                    };
                    await addJobCommand.HandleAsync(new(villageId, warehousePlan.ToJob(), true), cancellationToken);
                    await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
                    return Skip.WaitingStorageUpgrade;
                }
                if (granaryError != null)
                {
                    // Find the lowest level granary
                    var granary = await getLowestBuildingQuery.HandleAsync(new(villageId, BuildingEnums.Granary), cancellationToken);
                    // Check if granary upgrade is already in queue
                    var layoutBuildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId), cancellationToken);
                    var granaryBuilding = layoutBuildings.FirstOrDefault(x => x.Type == BuildingEnums.Granary && x.Location == granary.Location);
                    if (granaryBuilding != null && (granaryBuilding.QueueLevel > granaryBuilding.CurrentLevel || granaryBuilding.JobLevel > granaryBuilding.CurrentLevel))
                    {
                        // Wait for upgrade to finish
                        return Skip.WaitingStorageUpgrade;
                    }
                    // Queue granary upgrade
                    var granaryPlan = new NormalBuildPlan()
                    {
                        Location = granary.Location,
                        Type = granary.Type,
                        Level = granary.Level + 1,
                    };
                    await addJobCommand.HandleAsync(new(villageId, granaryPlan.ToJob(), true), cancellationToken);
                    await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
                    return Skip.WaitingStorageUpgrade;
                }
                return Stop.NotEnoughStorageCapacity;
            }

            var useHeroResource = settingService.BooleanByName(villageId, VillageSettingEnums.UseHeroResourceForBuilding);

            if (!useHeroResource && result.HasError<ResourceMissing>(out var resourceMissingErrors))
            {
                var message = string.Join(Environment.NewLine, resourceMissingErrors.Select(x => x.Message));
                logger.Warning("{Error}", message);

                return Skip.NotEnoughResource;
            }

            logger.Information("Use hero resource to upgrade building");
            var missingResource = storage.GetMissingResource(requiredResource);

            var url = browser.CurrentUrl;

            result = await toHeroInventoryCommand.HandleAsync(new(accountId), cancellationToken);
            if (result.IsFailed) return result;

            result = await updateInventoryCommand.HandleAsync(new(accountId), cancellationToken);
            if (result.IsFailed) return result;

            result = await useHeroResourceCommand.HandleAsync(new(accountId, missingResource), cancellationToken);

            await browser.Navigate(url, cancellationToken);

            if (result.IsFailed)
            {
                if (result.HasError<Retry>()) return result;

                if (result.HasError<ResourceMissing>(out var errors))
                {
                    var message = string.Join(Environment.NewLine, errors.Select(x => x.Message));
                    logger.Warning("{Error}", message);
                }

                return Skip.NotEnoughResource;
            }

            return Result.Ok();
        }

        private static long[] GetRequiredResource(IChromeBrowser browser, BuildingEnums building)
        {
            var doc = browser.Html;

            var resources = UpgradeParser.GetRequiredResource(doc, building);
            if (resources is null || resources.Count != 5) return new long[5];

            var resourceBuilding = new long[5];
            for (var i = 0; i < 5; i++)
            {
                resourceBuilding[i] = resources[i].InnerText.ParseLong();
            }

            return resourceBuilding;
        }
    }
}