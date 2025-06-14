using MainCore.Constraints;
using MainCore.Parsers;
using MainCore.Commands.Misc;
using MainCore.Notifications.Message;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class HandleUpgradeCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, NormalBuildPlan Plan, JobId JobId) : IAccountVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            DeleteJobByIdCommand.Handler deleteJobByIdCommand,
            JobUpdated.Handler jobUpdated,
            CancellationToken cancellationToken
        )
        {
            var (accountId, villageId, plan, jobId) = command;

            Result result;

            var upgradingLevel = UpgradeParser.GetUpgradingLevel(browser.Html);
            var nextLevel = UpgradeParser.GetNextLevel(browser.Html, plan.Type);
            if ((upgradingLevel.HasValue && upgradingLevel.Value >= plan.Level) ||
                (nextLevel.HasValue && nextLevel.Value >= plan.Level))
            {
                await deleteJobByIdCommand.HandleAsync(new(villageId, jobId), cancellationToken);
                await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
                return Continue.Error;
            }

            if (context.IsUpgradeable(villageId, plan))
            {
                var isSpecialUpgrade = context.BooleanByName(villageId, VillageSettingEnums.UseSpecialUpgrade);
                var isSpecialUpgradeable = context.IsSpecialUpgradeable(villageId, plan);
                if (isSpecialUpgrade && isSpecialUpgradeable)
                {
                    result = await browser.SpecialUpgrade(cancellationToken);
                    if (result.IsFailed) return result;
                }
                else
                {
                    result = await browser.Upgrade(cancellationToken);
                    if (result.IsFailed) return result;
                }
            }
            else
            {
                result = await browser.Construct(plan.Type, cancellationToken);
                if (result.IsFailed) return result;
            }

            // Building information will be updated by the task after this command
            return Result.Ok();
        }

        private static bool IsUpgradeable(this AppDbContext context, VillageId villageId, NormalBuildPlan plan)
        {
            return !context.IsEmptySite(villageId, plan.Location);
        }

        private static bool IsSpecialUpgradeable(
            this AppDbContext context,
            VillageId villageId,
            NormalBuildPlan plan
        )
        {
            if (plan.Type.IsResourceField())
            {
                var dto = context.GetBuilding(villageId, plan.Location);
                if (dto.Level == 0) return false;
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
            this IChromeBrowser browser,
            CancellationToken cancellationToken
        )
        {
            var html = browser.Html;
            var button = UpgradeParser.GetSpecialUpgradeButton(html);
            if (button is null) return Retry.ButtonNotFound("Watch ads upgrade");

            var result = await browser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;

            result = await browser.HandleAds(cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async Task<Result> HandleAds(
            this IChromeBrowser browser,
            CancellationToken cancellationToken
        )
        {
            var driver = browser.Driver;
            var current = driver.CurrentWindowHandle;
            while (driver.WindowHandles.Count > 1)
            {
                var others = driver.WindowHandles.First(x => !x.Equals(current));
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

            var result = await browser.Wait(videoFeatureShown, cancellationToken);
            if (result.IsFailed) return result;

            var html = browser.Html;
            var videoFeature = html.GetElementbyId("videoFeature");
            if (videoFeature.HasClass("infoScreen"))
            {
                var checkbox = videoFeature.Descendants("div").FirstOrDefault(x => x.HasClass("checkbox"));
                if (checkbox is null) return Retry.ButtonNotFound("Don't show watch ads confirm again");
                result = await browser.Click(By.XPath(checkbox.XPath));
                if (result.IsFailed) return result;

                var watchButton = videoFeature.Descendants("button").FirstOrDefault(x => x.HasClass("green"));
                if (watchButton is null) return Retry.ButtonNotFound("Watch ads");
                result = await browser.Click(By.XPath(watchButton.XPath));
                if (result.IsFailed) return result;
            }

            await Task.Delay(Random.Shared.Next(20_000, 25_000), CancellationToken.None);

            html = browser.Html;
            var node = html.GetElementbyId("videoFeature");
            if (node is null) return Retry.ButtonNotFound($"play ads");

            result = await browser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result;

            driver.SwitchTo().DefaultContent();

            // close if bot click on playing ads
            // chrome will open new tab & pause ads
            do
            {
                var handles = driver.WindowHandles;
                if (handles.Count <= 1) break;

                current = driver.CurrentWindowHandle;
                var other = driver.WindowHandles.First(x => !x.Equals(current));
                driver.SwitchTo().Window(other);
                driver.Close();
                driver.SwitchTo().Window(current);

                result = await browser.Click(By.XPath(node.XPath));
                if (result.IsFailed) return result;

                driver.SwitchTo().DefaultContent();
            }
            while (true);
            result = await browser.WaitPageChanged("dorf", cancellationToken);
            if (result.IsFailed) return result;

            await Task.Delay(Random.Shared.Next(5_000, 10_000), CancellationToken.None);

            html = browser.Html;
            var dontShowThisAgain = html.GetElementbyId("dontShowThisAgain");
            if (dontShowThisAgain is not null)
            {
                result = await browser.Click(By.XPath(dontShowThisAgain.XPath));
                if (result.IsFailed) return result;

                var okButton = html.DocumentNode.Descendants("button").FirstOrDefault(x => x.HasClass("dialogButtonOk"));
                if (okButton is null) return Retry.ButtonNotFound("ok");
                result = await browser.Click(By.XPath(okButton.XPath));
                if (result.IsFailed) return result;
            }

            return Result.Ok();
        }

        private static async Task<Result> Upgrade(
            this IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var html = browser.Html;

            var button = UpgradeParser.GetUpgradeButton(html);
            if (button is null) return Retry.ButtonNotFound("upgrade");

            var result = await browser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("dorf", cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async Task<Result> Construct(
            this IChromeBrowser browser,
            BuildingEnums building,
            CancellationToken cancellationToken
        )
        {
            var html = browser.Html;

            var button = UpgradeParser.GetConstructButton(html, building);
            if (button is null) return Retry.ButtonNotFound("construct");

            var result = await browser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("dorf", cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}