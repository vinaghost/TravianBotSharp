using MainCore.Commands.Features.NPC;
using MainCore.Commands.Navigate;
using MainCore.Infrasturecture.Extensions;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class NpcTask(
        ToBuildingByTypeCommand.Handler toBuildingCommand,
        OpenNPCPanelCommand.Handler openNpcPanelCommand,
        NPCCommand.Handler npcCommand,
        IDbContextFactory<AppDbContext> contextFactory,
        IChromeBrowser browser,
        ILogger logger,
        TaskManager taskManager)
    {
        public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
        {
            protected override string TaskName => "NPC";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(VillageId, VillageSettingEnums.AutoNPCEnable);
                if (!settingEnable) return false;

                var gold = context.AccountsInfo
                    .Where(x => x.AccountId == AccountId.Value)
                    .Select(x => x.Gold)
                    .FirstOrDefault();
                if (gold < 3) return false;

                var granaryPercent = (int)context.Storages
                   .Where(x => x.VillageId == VillageId.Value)
                   .Select(x => x.Crop * 100f / x.Granary)
                   .FirstOrDefault();

                var autoNPCGranaryPercent = context.ByName(VillageId, VillageSettingEnums.AutoNPCGranaryPercent);
                if (granaryPercent < autoNPCGranaryPercent) return false;

                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
        {
            Result result;
            result = await toBuildingCommand.HandleAsync(new(task.VillageId, BuildingEnums.Marketplace), cancellationToken);
            if (result.IsFailed)
            {
                if (result.HasError<MissingBuilding>())
                {
                    await TurnOffNPC(task.VillageId);
                    return Skip.Error.WithErrors(result.Errors);
                }
                return result;
            }

            if (!await CanNPC(task.VillageId))
            {
                return Result.Ok();
            }

            result = await openNpcPanelCommand.HandleAsync(new(browser), cancellationToken);
            if (result.IsFailed) return result;

            var values = await GetValues(task.VillageId);

            result = await CheckOverflow(task.VillageId, values);
            if (result.IsFailed)
            {
                await TurnOffNPC(task.VillageId);
                return Skip.Error.WithErrors(result.Errors);
            }

            logger.Information("Current resource:");
            await LogResource();

            result = await npcCommand.HandleAsync(new(browser, values), cancellationToken);
            if (result.IsFailed) return result;

            logger.Information("After NPC:");
            await LogResource();

            taskManager.AddOrUpdate(new UpgradeBuildingTask.Task(task.AccountId, task.VillageId));

            return Result.Ok();
        }

        private async ValueTask TurnOffNPC(VillageId villageId)
        {
            var settings = new Dictionary<VillageSettingEnums, int>() {
                        { VillageSettingEnums.AutoNPCEnable, 0 }
                    };
            using var context = contextFactory.CreateDbContext();
            context.SaveVillageSetting(villageId, settings);
            logger.Warning("Disable NPC for this village.");
        }

        private static readonly List<VillageSettingEnums> SettingNames =
        [
            VillageSettingEnums.AutoNPCWood,
            VillageSettingEnums.AutoNPCClay,
            VillageSettingEnums.AutoNPCIron,
            VillageSettingEnums.AutoNPCCrop,
        ];

        private async ValueTask<bool> CanNPC(VillageId villageId)
        {
            var storage = await StorageParser.GetStorage(browser.CurrentPage);

            var granaryPercent = (int)(storage.Crop * 100f / storage.Granary);

            using var context = contextFactory.CreateDbContext();
            var autoNPCGranaryPercent = context.ByName(villageId, VillageSettingEnums.AutoNPCGranaryPercent);

            if (granaryPercent < autoNPCGranaryPercent)
            {
                logger.Information("NPC resources not available. Granary percent is too low: {GranaryPercent} < {AutoNPCGranaryPercent}",
                    granaryPercent, autoNPCGranaryPercent);
                return false;
            }

            return true;
        }

        private async ValueTask<Result> CheckOverflow(VillageId villageId, long[] values)
        {
            using var context = contextFactory.CreateDbContext();
            var overflowNPC = context.BooleanByName(villageId, VillageSettingEnums.AutoNPCOverflow);
            if (overflowNPC) return Result.Ok();

            var storage = await StorageParser.GetStorage(browser.CurrentPage);
            for (var i = 0; i < 3; i++)
            {
                if (values[i] > storage.Warehouse)
                {
                    return StorageLimit.Warehouse(storage.Warehouse, values[i]);
                }
            }

            return Result.Ok();
        }

        private async ValueTask LogResource()
        {
            var storage = await StorageParser.GetStorage(browser.CurrentPage);

            logger.Information("[{Warehouse}]: {Wood} - {Clay} - {Iron} | [{Granary}]: {Crop}", storage.Warehouse, storage.Wood, storage.Clay, storage.Iron, storage.Granary, storage.Crop);
        }

        private async ValueTask<long[]> GetValues(VillageId villageId)
        {
            using var context = contextFactory.CreateDbContext();
            var settings = context.ByName(villageId, SettingNames);
            var ratio = GetRatio(settings);

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
                ratio = [.. Enumerable.Repeat<long>(1, 4)];
            }

            return ratio;
        }
    }
}