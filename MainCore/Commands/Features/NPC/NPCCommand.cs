using MainCore.Infrasturecture.Extensions;

namespace MainCore.Commands.Features.NPC
{
    [Handler]
    public sealed partial class NPCCommand(
        ChromeManager chromeManager,
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger logger)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand
        {
            public string Name => $"{nameof(NPCCommand)}";
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var (accountId, villageId) = command;
            var browser = chromeManager.Get(accountId);
            Result result;

            var values = await GetValues(villageId, browser.CurrentPage);

            result = await CheckOverflow(villageId, browser.CurrentPage, values);
            if (result.IsFailed) return result;

            await LogResource(browser.CurrentPage);

            result = await InputAmount(browser, values);
            if (result.IsFailed) return result;

            result = await browser.Click(NpcResourceParser.GetDistributeButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Click(NpcResourceParser.GetRedeemButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            await LogResource(browser.CurrentPage, after: true);

            return Result.Ok();
        }

        private static readonly List<VillageSettingEnums> SettingNames =
        [
            VillageSettingEnums.AutoNPCWood,
            VillageSettingEnums.AutoNPCClay,
            VillageSettingEnums.AutoNPCIron,
            VillageSettingEnums.AutoNPCCrop,
        ];

        private async ValueTask<long[]> GetValues(VillageId villageId, IPage page)
        {
            using var context = contextFactory.CreateDbContext();
            var settings = context.ByName(villageId, SettingNames);
            var ratio = GetRatio(settings);

            var sum = await NpcResourceParser.GetSum(page);
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
                ratio = [.. Enumerable.Repeat<long>(1, 4)];
            }

            return ratio;
        }

        private async ValueTask<Result> CheckOverflow(VillageId villageId, IPage page, long[] values)
        {
            using var context = contextFactory.CreateDbContext();
            var overflowNPC = context.BooleanByName(villageId, VillageSettingEnums.AutoNPCOverflow);
            if (overflowNPC) return Result.Ok();

            var storage = await StorageParser.GetStorage(page);
            for (var i = 0; i < 3; i++)
            {
                if (values[i] > storage.Warehouse)
                {
                    return StorageLimit.Warehouse(storage.Warehouse, values[i]);
                }
            }

            return Result.Ok();
        }

        private async ValueTask LogResource(IPage page, bool after = false)
        {
            if (after)
            {
                logger.Information("After NPC:");
            }
            else
            {
                logger.Information("Before NPC:");
            }

            var storage = await StorageParser.GetStorage(page);
            logger.Information("[{Warehouse}]: {Wood} - {Clay} - {Iron} | [{Granary}]: {Crop}", storage.Warehouse, storage.Wood, storage.Clay, storage.Iron, storage.Granary, storage.Crop);
        }

        private static async ValueTask<Result> InputAmount(IChromeBrowser browser, long[] values)
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
    }
}