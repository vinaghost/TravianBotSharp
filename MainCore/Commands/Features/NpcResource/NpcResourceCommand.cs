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
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            var villageId = command.VillageId;

            if (!await CanStart(browser, context, villageId))
            {
                return Result.Ok();
            }

            var result = await OpenNPCDialog(browser);
            if (result.IsFailed) return result;

            var settings = context.ByName(villageId, SettingNames);
            var ratio = GetRatio(settings);
            var values = await GetValues(browser, ratio);

            var warehouse = await StorageParser.GetWarehouseCapacity(browser.CurrentPage);
            var overflowNPC = context.BooleanByName(villageId, VillageSettingEnums.AutoNPCOverflow);
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
            await LogResource(browser);

            if (overflowNPC)
            {
                result = await Distribute(browser);
                if (result.IsFailed) return result;
            }

            result = await Redeem(browser);
            if (result.IsFailed) return result;

            await Task.Delay(5000);

            browser.Logger.Information("After NPC:");
            await LogResource(browser);

            return Result.Ok();
        }

        private static async Task LogResource(IChromeBrowser browser)
        {
            var wood = await StorageParser.GetWood(browser.CurrentPage);
            var clay = await StorageParser.GetClay(browser.CurrentPage);
            var iron = await StorageParser.GetIron(browser.CurrentPage);
            var crop = await StorageParser.GetCrop(browser.CurrentPage);

            var warehouse = await StorageParser.GetWarehouseCapacity(browser.CurrentPage);
            var granary = await StorageParser.GetGranaryCapacity(browser.CurrentPage);

            browser.Logger.Information("[{Warehouse}]: {Wood} - {Clay} - {Iron} | [{Granary}]: {Crop}", warehouse, wood, clay, iron, granary, crop);
        }

        private static async Task<bool> CanStart(IChromeBrowser browser, AppDbContext context, VillageId villageId)
        {
            var crop = await StorageParser.GetCrop(browser.CurrentPage);
            var granary = await StorageParser.GetGranaryCapacity(browser.CurrentPage);

            var granaryPercent = (int)(crop * 100f / granary);

            var autoNPCGranaryPercent = context.ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);
            if (granaryPercent < autoNPCGranaryPercent)
            {
                browser.Logger.Information("NPC resources not available. Granary percent is too low: {GranaryPercent} < {AutoNPCGranaryPercent}",
                    granaryPercent, autoNPCGranaryPercent);
                return false;
            }

            return true;
        }

        private static async Task<Result> OpenNPCDialog(IChromeBrowser browser)
        {
            var result = await browser.Click(NpcResourceParser.GetExchangeResourcesButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Wait(NpcResourceParser.NpcDialog(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async Task<Result> InputAmount(IChromeBrowser browser, long[] values)
        {
            var inputs = NpcResourceParser.GetInputs(browser.CurrentPage);

            var inputCount = await inputs.CountAsync();

            if (inputCount != 4)
            {
                return Stop.Error.WithError($"Expected 4 input elements, but found {inputCount}.");
            }

            for (var i = 0; i < 4; i++)
            {
                var inputElement = inputs.Nth(i);
                var result = await browser.Input(inputElement, $"{values[i]}");
                if (result.IsFailed) return result;
            }

            return Result.Ok();
        }

        private static async Task<long[]> GetValues(IChromeBrowser browser, long[] ratio)
        {
            var sum = await NpcResourceParser.GetSum(browser.CurrentPage);
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

        private static async Task<Result> Distribute(IChromeBrowser browser)
        {
            var result = await browser.Click(NpcResourceParser.GetDistributeButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async Task<Result> Redeem(IChromeBrowser browser)
        {
            var result = await browser.Click(NpcResourceParser.GetRedeemButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}