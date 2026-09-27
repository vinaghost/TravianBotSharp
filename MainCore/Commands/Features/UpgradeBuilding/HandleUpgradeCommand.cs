using Polly;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public sealed partial class HandleUpgradeCommand(
        IChromeBrowser browser,
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger logger)
    {
        public sealed record Command(VillageId VillageId, NormalBuildPlan Plan) : IVillageCommand;

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var (villageId, plan) = command;

            LogBuildingInfo(villageId, plan);

            Result result;
            if (CanUseSpecialUpgrade(villageId) && !UnskippableBuildings.Contains(plan.Type))
            {
                result = await SpecialUpgrade(plan.Type);
                if (result.IsFailed) return result;
            }
            else
            {
                result = await Upgrade(plan.Type);
                if (result.IsFailed) return result;
            }

            return Result.Ok();
        }

        private static readonly List<BuildingEnums> UnskippableBuildings =
        [
            BuildingEnums.Residence,
            BuildingEnums.Palace,
            BuildingEnums.CommandCenter,
        ];

        private void LogBuildingInfo(VillageId villageId, NormalBuildPlan plan)
        {
            using var context = contextFactory.CreateDbContext();

            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault(x => x.Location == plan.Location);

            if (queueBuilding is not null)
            {
                logger.Information("{Type} at location {Location} is in queue at level {Level}", queueBuilding.Type, queueBuilding.Location, queueBuilding.Level);
            }
            else
            {
                var building = context.Buildings
                   .Where(x => x.VillageId == villageId.Value)
                   .FirstOrDefault(x => x.Location == plan.Location);

                if (building is not null)
                {
                    logger.Information("{Type} at location {Location} is at level {Level}", building.Type, building.Location, building.Level);
                }
            }
        }

        private bool CanUseSpecialUpgrade(VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            var useSpecialUpgrade = context.BooleanByName(villageId, VillageSettingEnums.UseSpecialUpgrade);
            return useSpecialUpgrade;
        }

        private async Task<Result> SpecialUpgrade(BuildingEnums building)
        {
            var button = await UpgradeParser.GetSpecialButton(browser.CurrentPage, building);
            var result = await browser.Click(button);
            if (result.IsFailed) return result;

            result = await HandleAds();
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async Task<Result> HandleAds()
        {
            var page = browser.CurrentPage;
            var videoFeature = page.Locator("#videoFeature");

            var result = await browser.Wait(videoFeature);
            if (result.IsFailed) return result;

            var classess = await videoFeature.GetAttributeAsync("class") ?? "";
            if (classess.Contains("infoScreen"))
            {
                var checkBoxDontShowAgain = page.Locator("#videoFeature div.checkbox");
                result = await browser.Click(checkBoxDontShowAgain);
                if (result.IsFailed) return result;

                var buttonWatchAds = page.Locator("#videoFeature button.green");
                result = await browser.Click(buttonWatchAds);
                if (result.IsFailed) return result;
            }

            result = await browser.WaitPageChanged("dorf");
            if (result.IsFailed) return result;

            await Task.Delay(Random.Shared.Next(5_000, 10_000), CancellationToken.None);

            var dontShowThisAgain = page.Locator("#dontShowThisAgain");
            if (await dontShowThisAgain.CountAsync() > 0)
            {
                result = await browser.Click(dontShowThisAgain);
                if (result.IsFailed) return result;

                var okButton = page.Locator("button.dialogButtonOk");
                result = await browser.Click(okButton);
                if (result.IsFailed) return result;
            }

            return Result.Ok();
        }

        private async Task<Result> Upgrade(BuildingEnums building)
        {
            var button = await UpgradeParser.GetNormalButton(browser.CurrentPage, building);
            var result = await browser.Click(button);
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("dorf");
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}