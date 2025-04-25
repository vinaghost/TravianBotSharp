using MainCore.Commands.Abstract;
using MainCore.Common.Models;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [RegisterScoped<HandleUpgradeCommand>]
    public class HandleUpgradeCommand : CommandBase, ICommand<NormalBuildPlan>
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly DelayClickCommand _delayClickCommand;
        private readonly IGetSetting _getSetting;
        private readonly GetBuilding.Handler _getBuilding;

        private readonly List<BuildingEnums> _buildings = [
            BuildingEnums.Residence,
            BuildingEnums.Palace,
            BuildingEnums.CommandCenter
        ];

        public HandleUpgradeCommand(IDataService dataService, IDbContextFactory<AppDbContext> contextFactory, DelayClickCommand delayClickCommand, IGetSetting getSetting, GetBuilding.Handler getBuilding) : base(dataService)
        {
            _contextFactory = contextFactory;
            _delayClickCommand = delayClickCommand;
            _getSetting = getSetting;
            _getBuilding = getBuilding;
        }

        public async Task<Result> Execute(NormalBuildPlan plan, CancellationToken cancellationToken)
        {
            Result result;

            if (IsUpgradeable(plan))
            {
                if (IsSpecialUpgrade() && await IsSpecialUpgradeable(plan))
                {
                    result = await SpecialUpgrade(cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
                else
                {
                    result = await Upgrade(cancellationToken);
                    if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
            }
            else
            {
                result = await Construct(plan.Type, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            return Result.Ok();
        }

        private bool IsUpgradeable(NormalBuildPlan plan)
        {
            return !IsEmptySite(plan.Location);
        }

        private async Task<bool> IsSpecialUpgradeable(NormalBuildPlan plan)
        {
            if (_buildings.Contains(plan.Type)) return false;

            if (plan.Type.IsResourceField())
            {
                var villageId = _dataService.VillageId;
                var dto = await _getBuilding.HandleAsync(new(villageId, plan.Location));
                if (dto.Level == 0) return false;
            }
            return true;
        }

        private bool IsSpecialUpgrade()
        {
            var villageId = _dataService.VillageId;
            var useSpecialUpgrade = _getSetting.BooleanByName(villageId, VillageSettingEnums.UseSpecialUpgrade);
            return useSpecialUpgrade;
        }

        private bool IsEmptySite(int location)
        {
            var villageId = _dataService.VillageId;
            using var context = _contextFactory.CreateDbContext();
            bool isEmptySite = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == location)
                .Where(x => x.Type == BuildingEnums.Site || x.Level == -1)
                .Any();

            return isEmptySite;
        }

        private async Task<Result> SpecialUpgrade(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var button = UpgradeParser.GetSpecialUpgradeButton(html);
            if (button is null) return Retry.ButtonNotFound("Watch ads upgrade");

            var result = await chromeBrowser.Click(By.XPath(button.XPath), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var driver = chromeBrowser.Driver;
            var current = driver.CurrentWindowHandle;
            while (driver.WindowHandles.Count > 1)
            {
                var others = driver.WindowHandles.FirstOrDefault(x => !x.Equals(current));
                driver.SwitchTo().Window(others);
                driver.Close();
                driver.SwitchTo().Window(current);
            }

            static bool videoFeatureShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return doc.GetElementbyId("videoFeature") is not null;
            }

            result = await chromeBrowser.Wait(videoFeatureShown, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            html = chromeBrowser.Html;
            var videoFeature = html.GetElementbyId("videoFeature");
            if (videoFeature.HasClass("infoScreen"))
            {
                var checkbox = videoFeature.Descendants("div").FirstOrDefault(x => x.HasClass("checkbox"));
                if (checkbox is null) return Retry.ButtonNotFound("Don't show watch ads confirm again");
                result = await chromeBrowser.Click(By.XPath(checkbox.XPath), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await _delayClickCommand.Execute(cancellationToken);

                var watchButton = videoFeature.Descendants("button").FirstOrDefault(x => x.HasClass("green"));
                if (watchButton is null) return Retry.ButtonNotFound("Watch ads");
                result = await chromeBrowser.Click(By.XPath(watchButton.XPath), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            await Task.Delay(Random.Shared.Next(20_000, 25_000), CancellationToken.None);

            html = chromeBrowser.Html;
            var node = html.GetElementbyId("videoFeature");
            if (node is null) return Retry.ButtonNotFound($"play ads");

            result = await chromeBrowser.Click(By.XPath(node.XPath), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            driver.SwitchTo().DefaultContent();

            // close if bot click on playing ads
            // chrome will open new tab & pause ads
            do
            {
                var handles = driver.WindowHandles;
                if (handles.Count == 1) break;

                current = driver.CurrentWindowHandle;
                var other = driver.WindowHandles.FirstOrDefault(x => !x.Equals(current));
                driver.SwitchTo().Window(other);
                driver.Close();
                driver.SwitchTo().Window(current);

                result = await chromeBrowser.Click(By.XPath(node.XPath), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                driver.SwitchTo().DefaultContent();
            }
            while (true);

            result = await chromeBrowser.WaitPageChanged("dorf", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            await Task.Delay(Random.Shared.Next(5_000, 10_000), CancellationToken.None);

            html = chromeBrowser.Html;
            var dontShowThisAgain = html.GetElementbyId("dontShowThisAgain");
            if (dontShowThisAgain is not null)
            {
                result = await chromeBrowser.Click(By.XPath(dontShowThisAgain.XPath), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await _delayClickCommand.Execute(cancellationToken);

                var okButton = html.DocumentNode.Descendants("button").FirstOrDefault(x => x.HasClass("dialogButtonOk"));
                if (okButton is null) return Retry.ButtonNotFound("ok");
                result = await chromeBrowser.Click(By.XPath(okButton.XPath), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            return Result.Ok();
        }

        private async Task<Result> Upgrade(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;

            var button = UpgradeParser.GetUpgradeButton(html);
            if (button is null) return Retry.ButtonNotFound("upgrade");

            var result = await chromeBrowser.Click(By.XPath(button.XPath), "dorf", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> Construct(BuildingEnums building, CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;

            var button = UpgradeParser.GetConstructButton(html, building);
            if (button is null) return Retry.ButtonNotFound("construct");

            var result = await chromeBrowser.Click(By.XPath(button.XPath), "dorf", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}