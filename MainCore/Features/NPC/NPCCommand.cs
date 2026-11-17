using MainCore.Features.Update;

namespace MainCore.Features.NPC;

[Handler]
public sealed partial class NPCCommand(
    IDbContextFactory<AppDbContext> contextFactory,
    ILogger logger)
{
    public sealed record Command(IChromeBrowser Browser, VillageId VillageId) : ICommand
    {
        public string Name => nameof(NPCCommand).Humanize();
    }

    private async ValueTask<Result> HandleAsync(Command command)
    {
        var (browser, villageId) = command;
        Result result;

        var values = await GetValues(villageId, browser.CurrentPage);
        logger.Information("NPC values: {Wood} - {Clay} - {Iron} - {Crop}", values[0], values[1], values[2], values[3]);

        result = await CheckOverflow(villageId, browser.CurrentPage, values);
        if (result.IsFailed) return result;

        await LogResource(browser.CurrentPage);

        result = await InputAmount(browser, values);
        if (result.IsFailed) return result;

        result = await browser.Click(browser.CurrentPage.GetDistributeButton());
        if (result.IsFailed) return result;

        result = await browser.Click(browser.CurrentPage.GetRedeemButton());
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

        var sum = await page.GetSum();
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

        var storage = await UpdateStorageCommand.GetData(page);

        for (var i = 0; i < 3; i++)
        {
            if (values[i] > storage.Warehouse)
            {
                logger.Warning("Warehouse doesn't have enough capacity, need {Required} but have {Storage} ({Diff})", values[i], storage.Warehouse, values[i] - storage.Warehouse);
                return StorageLimit.Error;
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

        var storage = await UpdateStorageCommand.GetData(page);
        logger.Information("[{Warehouse}]: {Wood} - {Clay} - {Iron} | [{Granary}]: {Crop}", storage.Warehouse, storage.Wood, storage.Clay, storage.Iron, storage.Granary, storage.Crop);
    }

    private async ValueTask<Result> InputAmount(IChromeBrowser browser, long[] values)
    {
        var inputs = browser.CurrentPage.GetInputs();

        var inputCount = await inputs.CountAsync();

        if (inputCount != 4)
        {
            logger.Error("Expected 4 input elements, but found {InputCount}.", inputCount);
            return Stop.Error;
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