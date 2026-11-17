using MainCore.Features.InstantUpgrade;
using MainCore.Features.Navigate;

namespace MainCore.Features.Update;

public class BuildingDto
{
    public int Type { get; set; }
    public int Level { get; set; }
    public bool IsUnderConstruction { get; set; }
    public int Location { get; set; }
}

public class QueueBuildingDto
{
    public string Name { get; set; } = "";
    public int Position { get; set; }
    public int Type { get; set; } = -1;
    public int Level { get; set; }
    public int DurationSeconds { get; set; }
    public DateTime CompleteTime { get; set; }
}

[Handler]
public sealed partial class UpdateBuildingCommand(
    ILogger logger,
    IDbContextFactory<AppDbContext> contextFactory,
    TaskManager taskManager,
    RxQueue rxQueue,
    ToDorfCommand.Handler toDorfCommand)
{
    public sealed record Command(IChromeBrowser Browser, VillageId VillageId) : IVillageBrowserCommand
    {
        public string Name => "";
    }

    private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var (browser, accountId, villageId) = command;
        var first = await browser.CurrentPage.GetBuildingData();
        if (!BuildingSnapshotReconciler.IsValidPage(first))
        {
            logger.Warning("Cannot update buildings: the current page is not a valid village overview.");
            return Retry.Error;
        }

        var snapshots = new List<Parsers.BuildingDataDto> { first };
        var queueSnapshot = first;
        var resolved = BuildingSnapshotReconciler.ResolveQueue(first, first.Names);
        var needsOtherPage = !resolved || first.Queue.Any(x => ((BuildingEnums)x.Type).IsResourceField() != (first.Dorf == 1));
        if (!needsOtherPage)
        {
            using var context = contextFactory.CreateDbContext();
            needsOtherPage = context.QueueBuildings.AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => x.Type).AsEnumerable()
                .Any(type => type.IsResourceField() != (first.Dorf == 1));
        }
        if (needsOtherPage)
        {
            var result = await toDorfCommand.HandleAsync(new(browser, first.Dorf == 1 ? 2 : 1), cancellationToken);
            if (result.IsFailed) return result;

            var second = await browser.CurrentPage.GetBuildingData();
            result = await toDorfCommand.HandleAsync(new(browser, first.Dorf), cancellationToken);
            if (result.IsFailed) return result;

            if (!BuildingSnapshotReconciler.IsValidPage(second) || second.Dorf == first.Dorf || !BuildingSnapshotReconciler.SameQueue(first, second))
            {
                logger.Warning("Cannot update buildings: the other overview is invalid or the construction queue changed during refresh.");
                return Retry.Error;
            }
            snapshots.Add(second);
            queueSnapshot = second;
            if (!BuildingSnapshotReconciler.ResolveQueue(second, first.Names.Concat(second.Names)))
            {
                logger.Warning("Cannot resolve every construction queue entry: {QueueNames}", string.Join(", ", second.Queue.Where(x => x.Type <= 0 || x.Level <= 0 || x.CompleteTime == default).Select(x => x.Name)));
                return Retry.Error;
            }
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var tribe = context.VillagesSetting.FirstOrDefault(x => x.VillageId == villageId.Value && x.Setting == VillageSettingEnums.Tribe);
            if (tribe is not null && queueSnapshot.Tribe > 0) tribe.Value = queueSnapshot.Tribe;
            BuildingSnapshotReconciler.Apply(context, villageId, snapshots, queueSnapshot);
            var unresolvedLocations = context.QueueBuildings.Count(x => x.VillageId == villageId.Value && x.Location == -1);
            if (unresolvedLocations > 0)
                logger.Warning("Could not uniquely identify locations for {Count} construction queue entries in village {VillageId}.", unresolvedLocations, villageId.Value);
        }

        TriggerTask(accountId, villageId);
        rxQueue.Enqueue(new BuildingsModified(accountId, villageId));
        return Result.Ok();
    }

    private void TriggerTask(AccountId accountId, VillageId villageId)
    {
        using var context = contextFactory.CreateDbContext();
        if (!taskManager.IsExist<InstantUpgradeTask.Task>(accountId, villageId)) return;
        if (!context.IsInstantUpgradeEnable(accountId, villageId)) return;

        taskManager.Add(new InstantUpgradeTask.Task(accountId, villageId), true);
    }
}