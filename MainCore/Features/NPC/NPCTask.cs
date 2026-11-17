using MainCore.Features.Navigate;
using MainCore.Features.UpgradeBuilding;

namespace MainCore.Features.NPC;

[Handler]
public sealed partial class NPCTask(
    ChromeManager chromeManager,
    IDbContextFactory<AppDbContext> contextFactory,
    ILogger logger,
    CheckNPCCommand.Handler checkNPCCommand,
    ToBuildingByTypeCommand.Handler toBuildingCommand,
    OpenNPCPanelCommand.Handler openNPCPanelCommand,
    NPCCommand.Handler NPCCommand,
    TaskManager taskManager)
{
    public sealed class Task(AccountId accountId, VillageId villageId) : VillageTask(accountId, villageId)
    {
        protected override string TaskName => nameof(NPCTask).Humanize();
    }

    private async ValueTask<Result> HandleAsync(Task task, CancellationToken cancellationToken)
    {
        var browser = chromeManager.Get(task.AccountId);
        Result result;
        result = await toBuildingCommand.HandleAsync(new(browser, BuildingEnums.Marketplace), cancellationToken);
        if (result.IsFailed)
        {
            if (result.HasError<MissingBuilding>())
            {
                await TurnOffNPC(task.VillageId);
                return Skip.Error;
            }
            return result;
        }

        if (!await checkNPCCommand.HandleAsync(new(browser, task.VillageId), cancellationToken))
        {
            return Result.Ok();
        }

        result = await openNPCPanelCommand.HandleAsync(new(browser), cancellationToken);
        if (result.IsFailed) return result;

        result = await NPCCommand.HandleAsync(new(browser, task.VillageId), cancellationToken);
        if (result.IsFailed)
        {
            if (result.HasError<StorageLimit>())
            {
                await TurnOffNPC(task.VillageId);
                return Skip.Error;
            }
            return result;
        }

        taskManager.AddOrUpdate(new UpgradeBuildingTask.Task(task.AccountId, task.VillageId), true);

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