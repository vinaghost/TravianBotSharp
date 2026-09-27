using MainCore.Commands.Features.UseHeroItem;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public sealed partial class HandleResourceCommand(
        UpdateStorageCommand.Handler updateStorageCommand,
        UseHeroResourceCommand.Handler useHeroResourceCommand,
        ValidateEnoughResourceCommand.Handler validateEnoughResourceCommand,
        GetMissingResourceCommand.Handler getMissingResourceCommand,
        ISettingService settingService,
        IChromeBrowser browser,
        ILogger logger)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, NormalBuildPlan Plan) : IAccountVillageCommand;

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var (accountId, villageId, plan) = command;

            await updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            var requiredResource = await GetRequiredResource(browser, plan.Type);

            var result = await validateEnoughResourceCommand.HandleAsync(new(villageId, requiredResource), cancellationToken);
            if (!result.IsFailed) return Result.Ok();

            if (result.HasError<LackOfFreeCrop>()) return result;
            if (result.HasError<StorageLimit>()) return result;

            var useHeroResource = settingService.BooleanByName(villageId, VillageSettingEnums.UseHeroResourceForBuilding);
            if (!useHeroResource) return result;

            logger.Information("Don't have enough resource. Use resource in hero invetory to upgrade building");
            var missingResource = await getMissingResourceCommand.HandleAsync(new(villageId, requiredResource), cancellationToken);

            result = await useHeroResourceCommand.HandleAsync(new(plan.Type, missingResource), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async Task<long[]> GetRequiredResource(IChromeBrowser browser, BuildingEnums building)
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
    }
}