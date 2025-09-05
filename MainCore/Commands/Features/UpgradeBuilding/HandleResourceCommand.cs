using MainCore.Commands.Features.UseHeroItem;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class HandleResourceCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, NormalBuildPlan Plan) : IAccountVillageCommand
        {
            public void Deconstruct(out AccountId accountId, out VillageId villageId) => (accountId, villageId) = (AccountId, VillageId);
        }

        private static async ValueTask<Result> HandleAsync(
            Command command,
            UpdateStorageCommand.Handler updateStorageCommand,
            UseHeroResourceCommand.Handler useHeroResourceCommand,
            ValidateEnoughResourceCommand.Handler validateEnoughResourceCommand,
            GetMissingResourceCommand.Handler getMissingResourceCommand,
            ISettingService settingService,
            IBrowser browser,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId, plan) = command;

            await updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            var requiredResource = GetRequiredResource(browser, plan.Type);

            var result = await validateEnoughResourceCommand.HandleAsync(new(villageId, requiredResource), cancellationToken);
            if (!result.IsFailed) return Result.Ok();

            if (result.HasError<LackOfFreeCrop>()) return result;
            if (result.HasError<StorageLimit>()) return result;

            var useHeroResource = settingService.BooleanByName(villageId, VillageSettingEnums.UseHeroResourceForBuilding);
            if (!useHeroResource) return result;

            logger.Information("Don't have enough resource. Use resource in hero invetory to upgrade building");
            var missingResource = await getMissingResourceCommand.HandleAsync(new(villageId, requiredResource), cancellationToken);

            var url = browser.CurrentUrl;

            result = await useHeroResourceCommand.HandleAsync(new(accountId, missingResource), cancellationToken);
            await browser.Navigate(url, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static long[] GetRequiredResource(IBrowser browser, BuildingEnums building)
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
