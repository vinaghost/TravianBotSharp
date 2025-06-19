using MainCore.Constraints;
using MainCore.Parsers;

namespace MainCore.Commands.Features.NpcResource
{
    [Handler]
    public static partial class NpcResourceCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static readonly List<VillageSettingEnums> SettingNames = new()
        {
            VillageSettingEnums.AutoNPCWood,
            VillageSettingEnums.AutoNPCClay,
            VillageSettingEnums.AutoNPCIron,
            VillageSettingEnums.AutoNPCCrop,
        };

        private static async ValueTask<Result> HandleAsync(
            Command command,
            AppDbContext context,
            IChromeBrowser browser,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId) = command;

            var result = await OpenNPCDialog(browser, cancellationToken);
            if (result.IsFailed) return result;

            var html = browser.Html;
            var currentVillage = VillagePanelParser.GetCurrentVillageId(html);
            if (currentVillage != villageId) return Skip.WrongVillage;

            var percentSetting = settingService.ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);

            // Check granary level from current page to avoid using stale DB data
            var currentCrop = StorageParser.GetCrop(html);
            var granaryCapacity = StorageParser.GetGranaryCapacity(html);
            if (granaryCapacity > 0)
            {
                var percent = currentCrop * 100f / granaryCapacity;
                if (percent < percentSetting) return Skip.GranaryNotReady;
            }

            var settings = settingService.ByName(villageId, SettingNames);
            var ratio = GetRatio(settings, villageId);

            result = await InputAmount(browser, ratio);
            if (result.IsFailed) return result;

            result = await Redeem(browser, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async Task<Result> OpenNPCDialog(IChromeBrowser browser, CancellationToken cancellationToken)
        {
            var html = browser.Html;

            var button = NpcResourceParser.GetExchangeResourcesButton(html);
            if (button is null) return Retry.ButtonNotFound("Exchange resources");

            static bool DialogShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return NpcResourceParser.IsNpcDialog(doc);
            }

            var result = await browser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;

            result = await browser.Wait(DialogShown, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async Task<Result> InputAmount(IChromeBrowser browser, long[] ratio)
        {
            var html = browser.Html;

            var sum = NpcResourceParser.GetSum(html);
            var sumRatio = ratio.Sum();
            var values = new long[4];
            for (var i = 0; i < 4; i++)
            {
                values[i] = sum * ratio[i] / sumRatio;
            }
            var sumValue = values.Sum();
            var diff = sum - sumValue;
            values[3] += diff;

            var inputs = NpcResourceParser.GetInputs(html).ToArray();

            for (var i = 0; i < 4; i++)
            {
                var result = await browser.Input(By.XPath(inputs[i].XPath), $"{values[i]}");
                if (result.IsFailed) return result;
            }
            return Result.Ok();
        }

        private static long[] GetRatio(Dictionary<VillageSettingEnums, int> settings, VillageId villageId)
        {
            var ratio = new long[4]
            {
                settings[VillageSettingEnums.AutoNPCWood],
                settings[VillageSettingEnums.AutoNPCClay],
                settings[VillageSettingEnums.AutoNPCIron],
                settings[VillageSettingEnums.AutoNPCCrop],
            };
            var sum = ratio.Sum();
            if (sum == 0)
            {
                ratio = Enumerable.Repeat<long>(1, 4).ToArray();
            }

            return ratio;
        }

        private static async Task<Result> Redeem(IChromeBrowser browser, CancellationToken cancellationToken)
        {
            var html = browser.Html;

            var button = NpcResourceParser.GetRedeemButton(html);
            if (button is null)
            {
                button = NpcResourceParser.GetDistributeButton(html);
                if (button is null) return Retry.ButtonNotFound("redeem");
            }

            var result = await browser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;

            static bool DialogClosed(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return !NpcResourceParser.IsNpcDialog(doc);
            }

            result = await browser.Wait(DialogClosed, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}
