namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public sealed partial class HandleUpgradeCommand(
        ChromeManager chromeManager,
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger logger)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, NormalBuildPlan Plan) : IAccountVillageCommand
        {
            public string Name => $"{nameof(HandleUpgradeCommand)} [{Description}]";
            private string Description => $"{Plan}";
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var (accountId, villageId, plan) = command;

            LogBuildingInfo(villageId, plan);

            Result result;
            if (CanUseSpecialUpgrade(villageId) && !UnskippableBuildings.Contains(plan.Type))
            {
                result = await SpecialUpgrade(accountId, plan.Type);
                if (result.IsFailed) return result;
            }
            else
            {
                result = await Upgrade(accountId, plan.Type);
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

        private async Task<Result> SpecialUpgrade(AccountId accountId, BuildingEnums building, bool repeat = false)
        {
            var browser = chromeManager.Get(accountId);

            var button = await UpgradeParser.GetSpecialButton(browser.CurrentPage, building);
            var result = await browser.Click(button);
            if (result.IsFailed) return result;

            var videoFeature = browser.CurrentPage.Locator("#videoFeature");

            await Task.Delay(5_000);
            if (await videoFeature.CountAsync() == 0)
            {
                browser.Logger.Information("Video not shown. Refreshing the page.");
                result = await browser.Refresh();
                if (result.IsFailed) return result;

                result = await browser.Click(button);
                if (result.IsFailed) return result;
            }

            result = await browser.Wait(videoFeature);
            if (result.IsFailed) return result;

            result = await HandleAds(accountId, building, repeat);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async Task<Result> HandleAds(AccountId accountId, BuildingEnums building, bool repeat = false)
        {
            if (repeat)
            {
                return Retry.Error.WithError("Special upgrade failed after retrying.");
            }

            var browser = chromeManager.Get(accountId);
            var page = browser.CurrentPage;
            var videoFeature = page.Locator("#videoFeature");
            Result result;
            var classess = await videoFeature.GetAttributeAsync("class") ?? "";
            if (classess.Contains("infoScreen"))
            {
                var checkBoxDontShowAgain = page.Locator("#videoFeature label.checkbox input[name=\"preference\"]");
                result = await browser.Click(checkBoxDontShowAgain);
                if (result.IsFailed) return result;

                var buttonWatchAds = page.Locator("#videoFeature div.buttonWrapper button");
                result = await browser.Click(buttonWatchAds);
                if (result.IsFailed) return result;
            }

            await Task.Delay(10_000);

            var videoAreaChildrend = browser.CurrentPage.Locator("#videoArea > *");
            if (await videoAreaChildrend.CountAsync() == 0)
            {
                browser.Logger.Information("Video not loaded. Refreshing the page.");
                result = await browser.Refresh();
                if (result.IsFailed) return result;
                result = await SpecialUpgrade(accountId, building, true);
                if (result.IsFailed) return result;
                return Result.Ok();
            }

            result = await browser.WaitPageChanged("dorf");
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async Task<Result> Upgrade(AccountId accountId, BuildingEnums building)
        {
            var browser = chromeManager.Get(accountId);
            var button = await UpgradeParser.GetNormalButton(browser.CurrentPage, building);
            var result = await browser.Click(button);
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("dorf");
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}