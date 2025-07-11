namespace MainCore.Commands.Features.NpcResource
{
    [Handler]
    public static partial class NpcResourceCommand
    {
        public sealed record Command(VillageId VillageId) : IVillageCommand;

        private static readonly List<VillageSettingEnums> SettingNames = new()
        {
            VillageSettingEnums.AutoNPCWood,
            VillageSettingEnums.AutoNPCClay,
            VillageSettingEnums.AutoNPCIron,
            VillageSettingEnums.AutoNPCCrop,
        };

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            var villageId = command.VillageId;

            var result = await OpenNPCDialog(browser, cancellationToken);
            if (result.IsFailed) return result;

            var settings = settingService.ByName(villageId, SettingNames);
            var ratio = GetRatio(settings);
            var values = GetValues(browser, ratio);

            var warehouse = StorageParser.GetWarehouseCapacity(browser.Html);
            var overflowNPC = settingService.BooleanByName(villageId, VillageSettingEnums.AutoNPCOverflow);
            for (var i = 0; i < 3; i++)
            {
                if (values[i] > warehouse)
                {
                    if (overflowNPC)
                    {
                        return StorageLimit.Warehouse(warehouse, values[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            result = await InputAmount(browser, values);
            if (result.IsFailed) return result;

            browser.Logger.Information("Current resource:");
            LogResource(browser);

            if (overflowNPC)
            {
                result = await Distribute(browser, cancellationToken);
                if (result.IsFailed) return result;
            }

            result = await Redeem(browser, cancellationToken);
            if (result.IsFailed) return result;

            browser.Logger.Information("After NPC:");
            LogResource(browser);

            return Result.Ok();
        }

        private static void LogResource(IChromeBrowser browser)
        {
            var wood = StorageParser.GetWood(browser.Html);
            var clay = StorageParser.GetClay(browser.Html);
            var iron = StorageParser.GetIron(browser.Html);
            var crop = StorageParser.GetCrop(browser.Html);
            browser.Logger.Information("Wood: {Wood}, Clay: {Clay}, Iron: {Iron}, Crop: {Crop}", wood, clay, iron, crop);
        }

        private static async Task<Result> OpenNPCDialog(IChromeBrowser browser, CancellationToken cancellationToken)
        {
            var button = NpcResourceParser.GetExchangeResourcesButton(browser.Html);
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

        private static async Task<Result> InputAmount(IChromeBrowser browser, long[] values)
        {
            var inputs = NpcResourceParser.GetInputs(browser.Html).ToArray();

            for (var i = 0; i < 4; i++)
            {
                var result = await browser.Input(By.XPath(inputs[i].XPath), $"{values[i]}");
                if (result.IsFailed) return result;
            }

            return Result.Ok();
        }

        private static long[] GetValues(IChromeBrowser browser, long[] ratio)
        {
            var sum = NpcResourceParser.GetSum(browser.Html);
            var sumRatio = ratio.Sum();
            var values = new long[4];
            for (var i = 0; i < 4; i++)
            {
                values[i] = sum * ratio[i] / sumRatio;
            }
            var sumValue = values.Sum();
            var diff = sum - sumValue;
            values[3] += diff;
            return values;
        }

        private static long[] GetRatio(Dictionary<VillageSettingEnums, int> settings)
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

        private static async Task<Result> Distribute(IChromeBrowser browser, CancellationToken cancellationToken)
        {
            var result = await browser.Wait(driver =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var button = NpcResourceParser.GetDistributeButton(browser.Html);
                if (button is null) return false;

                var elements = driver.FindElements(By.XPath(button.XPath));
                return elements.Count > 0 && elements[0].Enabled;
            }, cancellationToken);

            var button = NpcResourceParser.GetDistributeButton(browser.Html);
            if (button is null) return Retry.ButtonNotFound("distribute");

            result = await browser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async Task<Result> Redeem(IChromeBrowser browser, CancellationToken cancellationToken)
        {
            var result = await browser.Wait(driver =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var button = NpcResourceParser.GetRedeemButton(doc);

                if (button is null) return false;

                var elements = driver.FindElements(By.XPath(button.XPath));
                return elements.Count > 0 && elements[0].Enabled;
            }, cancellationToken);

            var button = NpcResourceParser.GetRedeemButton(browser.Html);
            if (button is null) return Retry.ButtonNotFound("redeem");

            result = await browser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}