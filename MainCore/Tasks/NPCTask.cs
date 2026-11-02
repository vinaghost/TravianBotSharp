using MainCore.Commands.Features.NPC;
using MainCore.Commands.Navigate;
using MainCore.Infrasturecture.Extensions;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class NpcTask(
        CheckNPCCommand.Handler checkNPCCommand,
        ToBuildingByTypeCommand.Handler toBuildingCommand,
        OpenNPCPanelCommand.Handler openNPCPanelCommand,
        NPCCommand.Handler NPCCommand,
        IDbContextFactory<AppDbContext> contextFactory,
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
            result = await toBuildingCommand.HandleAsync(new(task.AccountId, task.VillageId, BuildingEnums.Marketplace), cancellationToken);
            if (result.IsFailed)
            {
                if (result.HasError<MissingBuilding>())
                {
                    await TurnOffNPC(task.VillageId);
                    return Skip.Error.WithErrors(result.Errors);
                }
                return result;
            }

            if (!await checkNPCCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken))
            {
                return Result.Ok();
            }

            result = await openNPCPanelCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            result = await NPCCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
            if (result.IsFailed)
            {
                if (result.HasError<StorageLimit>())
                {
                    await TurnOffNPC(task.VillageId);
                    return Skip.Error.WithErrors(result.Errors);
                }
                return result;
            }

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
    }
}