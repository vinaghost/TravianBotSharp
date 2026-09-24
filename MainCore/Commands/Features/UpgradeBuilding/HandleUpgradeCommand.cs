namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class HandleUpgradeCommand
    {
        public sealed record Command(VillageId VillageId, NormalBuildPlan Plan) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            ILogger logger,
            CancellationToken cancellationToken
        )
        {
            var (villageId, plan) = command;

            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .FirstOrDefault();

            if (queueBuilding is not null)
            {
                logger.Information("{Type} at location {Location} is in queue at level {Level}", queueBuilding.Type, queueBuilding.Location, queueBuilding.Level);
            }
            else
            {
                var building = context.Buildings
               .Where(x => x.VillageId == villageId.Value)
               .Where(x => x.Location == plan.Location)
               .FirstOrDefault();

                if (building is not null)
                {
                    logger.Information("{Type} at location {Location} is at level {Level}", building.Type, building.Location, building.Level);
                }
            }

            Result result;
            if (context.IsUpgradeable(villageId, plan))
            {
                var isSpecialUpgrade = context.BooleanByName(villageId, VillageSettingEnums.UseSpecialUpgrade);
                var isSpecialUpgradeable = context.IsSpecialUpgradeable(villageId, plan);
                if (isSpecialUpgrade && isSpecialUpgradeable)
                {
                    result = await browser.SpecialUpgrade();
                    if (result.IsFailed) return result;
                }
                else
                {
                    result = await browser.Upgrade();
                    if (result.IsFailed) return result;
                }
            }
            else
            {
                result = await browser.Construct(plan.Type);
                if (result.IsFailed) return result;
            }

            return Result.Ok();
        }

        private static bool IsUpgradeable(this AppDbContext context, VillageId villageId, NormalBuildPlan plan)
        {
            return !context.IsEmptySite(villageId, plan.Location);
        }

        private static List<BuildingEnums> UnskippableBuildings = new()
        {
            BuildingEnums.Residence,
            BuildingEnums.Palace,
            BuildingEnums.CommandCenter,
        };

        private static bool IsSpecialUpgradeable(
            this AppDbContext context,
            VillageId villageId,
            NormalBuildPlan plan
        )
        {
            if (UnskippableBuildings.Contains(plan.Type)) return false;

            if (plan.Type.IsResourceField())
            {
                var getBuildingSpec = new GetBuildingSpec(villageId, plan.Location);
                var level = context.Buildings
                    .WithSpecification(getBuildingSpec)
                    .Select(x => x.Level)
                    .FirstOrDefault();
                if (level == 0) return false;
            }
            return true;
        }

        private static bool IsEmptySite(this AppDbContext context, VillageId villageId, int location)
        {
            return context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == location)
                .Where(x => x.Type == BuildingEnums.Site || x.Level == -1)
                .Any();
        }

        private static async Task<Result> SpecialUpgrade(
            this IChromeBrowser browser
        )
        {
            var result = await browser.Click(UpgradeParser.GetSpecialUpgradeButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.HandleAds();
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async Task<Result> HandleAds(
            this IChromeBrowser browser
        )
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

        private static async Task<Result> Upgrade(
            this IChromeBrowser browser)
        {
            var result = await browser.Click(UpgradeParser.GetUpgradeButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("dorf");
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async Task<Result> Construct(
            this IChromeBrowser browser,
            BuildingEnums building
        )
        {
            var result = await browser.Click(UpgradeParser.GetConstructButton(browser.CurrentPage, building));
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("dorf");
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}