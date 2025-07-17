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

            if (result.HasError<LackOfFreeCrop>()) return result;
            if (result.HasError<StorageLimit>()) return result;

            var useHeroResource = settingService.BooleanByName(villageId, VillageSettingEnums.UseHeroResourceForBuilding);

            if (!useHeroResource && result.HasError<MissingResource>(out var MissingResourceErrors)) return Result.Fail(MissingResourceErrors);

            logger.Information("Don't have enough resource. Use resource in hero invetory to upgrade building");
            var missingResource = storage.GetMissingResource(requiredResource);

            var url = browser.CurrentUrl;

            result = await useHeroResourceCommand.HandleAsync(new(accountId, missingResource), cancellationToken);
            await browser.Navigate(url, cancellationToken);

            if (result.IsFailed) return result;

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