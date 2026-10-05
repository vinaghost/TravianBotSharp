using MainCore.Commands.Features.UseHeroItem;
using MainCore.Infrasturecture.Extensions;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public sealed partial class HandleResourceCommand(
        UpdateStorageCommand.Handler updateStorageCommand,
        UseHeroResourceCommand.Handler useHeroResourceCommand,
        RxQueue rxQueue,
        IDbContextFactory<AppDbContext> contextFactory,
        IChromeBrowser browser,
        ILogger logger)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, NormalBuildPlan Plan) : IAccountVillageCommand;

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var (accountId, villageId, plan) = command;

            await updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            var requiredResource = await GetRequiredResource(plan.Type);

            var result = IsEnoughResource(villageId, requiredResource);
            if (!result.IsFailed) return Result.Ok();

            if (result.HasError<LackOfFreeCrop>())
            {
                await AddCropland(accountId, villageId);
                return result;
            }

            if (result.HasError<StorageLimit>()) return result;
            if (!CanUseHeroResource(villageId)) return result;

            logger.Information("Don't have enough resource. Use resource in hero invetory to upgrade building");
            var missingResource = GetMissingResource(villageId, requiredResource);

            result = await useHeroResourceCommand.HandleAsync(new(plan.Type, missingResource), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private async Task<long[]> GetRequiredResource(BuildingEnums building)
        {
            var resources = await UpgradeParser.GetRequiredResource(browser.CurrentPage, building);
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

        private bool CanUseHeroResource(VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            return context.BooleanByName(villageId, VillageSettingEnums.UseHeroResourceForBuilding);
        }

        private long[] GetMissingResource(VillageId villageId, long[] requiredResource)
        {
            using var context = contextFactory.CreateDbContext();
            var storage = context.Storages
                .FirstOrDefault(x => x.VillageId == villageId.Value);

            if (storage is null) return [0, 0, 0, 0];

            var resource = new long[4];
            if (storage.Wood < requiredResource[0]) resource[0] = requiredResource[0] - storage.Wood;
            if (storage.Clay < requiredResource[1]) resource[1] = requiredResource[1] - storage.Clay;
            if (storage.Iron < requiredResource[2]) resource[2] = requiredResource[2] - storage.Iron;
            if (storage.Crop < requiredResource[3]) resource[3] = requiredResource[3] - storage.Crop;
            return resource;
        }

        private async Task AddCropland(AccountId accountId, VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            var buildings = context.GetLayoutBuildings(villageId, true);

            var cropland = buildings
                .Where(x => x.Type == BuildingEnums.Cropland)
                .OrderBy(x => x.Level)
                .First();

            var cropLandPlan = new NormalBuildPlan()
            {
                Location = cropland.Location,
                Type = cropland.Type,
                Level = cropland.Level + 1,
            };

            context.AddJob(villageId, cropLandPlan, true);
            rxQueue.Enqueue(new JobsModified(accountId, villageId));
        }

        private Result IsEnoughResource(VillageId villageId, long[] resource)
        {
            using var context = contextFactory.CreateDbContext();
            var storage = context.Storages
                .FirstOrDefault(x => x.VillageId == villageId.Value);

            if (storage is null) return Result.Ok();

            var errors = new List<Error>();
            if (storage.Wood < resource[0])
            {
                errors.Add(MissingResource.Wood(storage.Wood, resource[0]));
            }

            if (storage.Clay < resource[1])
            {
                errors.Add(MissingResource.Clay(storage.Clay, resource[1]));
            }

            if (storage.Iron < resource[2])
            {
                errors.Add(MissingResource.Iron(storage.Iron, resource[2]));
            }

            if (storage.Crop < resource[3])
            {
                errors.Add(MissingResource.Crop(storage.Crop, resource[3]));
            }

            if (resource.Length == 5 && storage.FreeCrop < resource[4])
            {
                errors.Add(LackOfFreeCrop.Error(storage.FreeCrop, resource[4]));
            }

            if (storage.Granary < resource[3])
            {
                errors.Add(StorageLimit.Granary(storage.Granary, resource[3]));
            }

            var max = resource.Take(3).Max();
            if (storage.Warehouse < max)
            {
                errors.Add(StorageLimit.Warehouse(storage.Warehouse, max));
            }

            return Result.FailIfNotEmpty(errors);
        }
    }
}