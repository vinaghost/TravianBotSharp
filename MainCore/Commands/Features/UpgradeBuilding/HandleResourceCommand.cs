using MainCore.Commands.Features.UseHeroItem;
using MainCore.Constraints;
using MainCore.Errors.AutoBuilder;
using MainCore.Errors.Storage;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class HandleResourceCommand
    {
        public sealed record Command(AccountId accountId, VillageId villageId, NormalBuildPlan Plan) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            JobUpdated.Handler jobUpdated,
            UpdateStorageCommand.Handler updateStorageCommand,
            UseHeroResourceCommand.Handler useHeroResourceCommand,
            ToHeroInventoryCommand.Handler toHeroInventoryCommand,
            UpdateInventoryCommand.Handler updateInventoryCommand,
            AddJobCommand.Handler addJobCommand,
            AppDbContext context,
            IChromeBrowser browser,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId, plan) = command;

            var storage = await updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            var requiredResource = GetRequiredResource(browser, plan.Type);

            Result result = storage.IsResourceEnough(requiredResource);
            if (!result.IsFailed) return Result.Ok();

            if (result.HasError<FreeCrop>(out var freeCropErrors))
            {
                var error = freeCropErrors.First();
                logger.Warning("{Error}", error);

                var cropland = context.GetLowestCropland(villageId);

                var cropLandPlan = new NormalBuildPlan()
                {
                    Location = cropland.Location,
                    Type = cropland.Type,
                    Level = cropland.Level + 1,
                };

                await addJobCommand.HandleAsync(new(accountId, villageId, cropLandPlan.ToJob(villageId), true), cancellationToken);
                logger.Information($"Add cropland to top of the job queue");
                return Continue.Error;
            }

            if (result.HasError<StorageLimit>(out var storageLimitErrors))
            {
                var error = storageLimitErrors.First();
                logger.Warning("{Error}", error);
                return Stop.NotEnoughStorageCapacity;
            }

            var useHeroResource = context.BooleanByName(villageId, VillageSettingEnums.UseHeroResourceForBuilding);

            if (!useHeroResource)
            {
                if (result.HasError<Resource>(out var resourceErrors))
                {
                    foreach (var error in resourceErrors)
                    {
                        logger.Warning("{Error}", error);
                    }
                }

                var time = UpgradeParser.GetTimeWhenEnoughResource(browser.Html, plan.Type);
                return WaitResource.Error(time);
            }

            logger.Information("Use hero resource to upgrade building");
            var missingResource = storage.GetMissingResource(requiredResource);

            var url = browser.CurrentUrl;

            result = await toHeroInventoryCommand.HandleAsync(new(accountId), cancellationToken);
            if (result.IsFailed) return result;

            result = await updateInventoryCommand.HandleAsync(new(accountId), cancellationToken);
            if (result.IsFailed) return result;

            var heroResourceResult = await useHeroResourceCommand.HandleAsync(new(accountId, missingResource), cancellationToken);
            await browser.Navigate(url, cancellationToken);

            if (heroResourceResult.IsFailed)
            {
                if (heroResourceResult.HasError<Retry>()) return heroResourceResult;

                if (heroResourceResult.HasError<Resource>(out var resourceErrors))
                {
                    foreach (var error in resourceErrors)
                    {
                        logger.Warning("{Error}", error);
                    }
                }

                var time = UpgradeParser.GetTimeWhenEnoughResource(browser.Html, plan.Type);
                return WaitResource.Error(time);
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

        private static Building GetLowestCropland(this AppDbContext context, VillageId villageId)
        {
            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == BuildingEnums.Cropland)
                .OrderBy(x => x.Level)
                .FirstOrDefault();
            return building;
        }
    }
}