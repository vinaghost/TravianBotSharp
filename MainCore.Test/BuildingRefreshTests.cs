using MainCore.Features.Update;
using MainCore.Infrasturecture;
using MainCore.Infrasturecture.Entities;
using MainCore.Shared.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using BuildingParser = MainCore.Features.Update.Parsers;

namespace MainCore.Test;

public class BuildingRefreshTests
{
    private static readonly DateTime Completion = new(2030, 1, 1, 12, 0, 0);

    [Fact]
    public void MixedQueue_ReusesLocalizedNamesAndStoredLocationWithoutOtherPage()
    {
        using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
        connection.Open();
        using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        context.Database.EnsureCreated();
        context.Buildings.Add(new Building { VillageId = 1, Location = 1, Type = BuildingEnums.Woodcutter, Level = 1, IsUnderConstruction = true });
        context.QueueBuildings.Add(new QueueBuilding { VillageId = 1, Location = 1, Type = BuildingEnums.Woodcutter, Level = 2, Position = 0, CompleteTime = Completion });
        context.SaveChanges();

        var cache = new BuildingNameCache();
        var account = new AccountId(1);
        var village = new VillageId(1);
        cache.Store(account, village, Snapshot(1, [], [], [new("Holz", 1)]));
        var snapshot = Snapshot(2, [Building(19, 19, 2)], [Queue("Holz", 2, 0), Queue("Kaserne", 3, 1)], [new("Kaserne", 19)]);
        cache.Store(account, village, snapshot);

        BuildingSnapshotReconciler.ResolveQueue(snapshot, cache.Get(account, village)).ShouldBeTrue();
        var previousQueue = context.QueueBuildings.AsNoTracking().ToList();
        BuildingSnapshotReconciler.HasVanishedOtherPageEntries(snapshot, previousQueue).ShouldBeFalse();
        BuildingSnapshotReconciler.Apply(context, village, [snapshot], snapshot);
        context.ChangeTracker.Clear();

        var entries = context.QueueBuildings.OrderBy(x => x.Position).ToArray();
        entries.Select(x => x.Location).ShouldBe(new[] { 1, 19 });
        context.Buildings.Single(x => x.Location == 1).Level.ShouldBe(1);
    }

    [Fact]
    public void VanishedOtherPageEntry_RefreshesEvenWhenDuplicateRemains()
    {
        var previous = new[]
        {
            Existing(BuildingEnums.Woodcutter, 2), Existing(BuildingEnums.Woodcutter, 2),
            Existing(BuildingEnums.Barracks, 3)
        };
        var snapshot = Snapshot(2, [], [Queue("Holz", 2, 0, 1), Queue("Kaserne", 3, 1, 19)], []);

        BuildingSnapshotReconciler.HasVanishedOtherPageEntries(snapshot, previous).ShouldBeTrue();
    }

    [Fact]
    public void VanishedVillageEntry_RequiresRefreshFromResourcePage()
    {
        var previous = new[] { Existing(BuildingEnums.Barracks, 3) };
        var snapshot = Snapshot(1, [], [Queue("Holz", 2, 0, 1)], []);

        BuildingSnapshotReconciler.HasVanishedOtherPageEntries(snapshot, previous).ShouldBeTrue();
    }

    [Fact]
    public void AdditionalOtherPageEntry_DoesNotCountAsDisappearance()
    {
        var previous = new[] { Existing(BuildingEnums.Woodcutter, 2) };
        var snapshot = Snapshot(2, [], [Queue("Holz", 2, 0, 1), Queue("Holz", 2, 1, 1)], []);

        BuildingSnapshotReconciler.HasVanishedOtherPageEntries(snapshot, previous).ShouldBeFalse();
    }

    [Fact]
    public void MissingCachedName_RequiresFurtherResolution()
    {
        var snapshot = Snapshot(2, [], [Queue("Holz", 2, 0)], [new("Kaserne", 19)]);
        var cache = new BuildingNameCache();
        cache.Store(new AccountId(1), new VillageId(1), snapshot);

        BuildingSnapshotReconciler.ResolveQueue(snapshot, cache.Get(new AccountId(1), new VillageId(1))).ShouldBeFalse();
        snapshot.Queue.Single().Type.ShouldBe(-1);
    }

    [Fact]
    public void VanishedCurrentPageEntry_DoesNotRequireOtherPage()
    {
        var previous = new[] { Existing(BuildingEnums.Woodcutter, 2), Existing(BuildingEnums.Barracks, 3) };
        var snapshot = Snapshot(2, [], [Queue("Holz", 2, 0, 1)], []);

        BuildingSnapshotReconciler.HasVanishedOtherPageEntries(snapshot, previous).ShouldBeFalse();
    }

    [Fact]
    public void UnchangedOtherPageDuplicates_KeepBothAssignedLocations()
    {
        using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
        connection.Open();
        using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        context.Database.EnsureCreated();
        context.QueueBuildings.AddRange(
            new QueueBuilding { VillageId = 1, Type = BuildingEnums.Woodcutter, Level = 2, Location = 1, Position = 0 },
            new QueueBuilding { VillageId = 1, Type = BuildingEnums.Woodcutter, Level = 2, Location = 2, Position = 1 });
        context.SaveChanges();
        var snapshot = Snapshot(2, [Building(19, 19, 2)], [Queue("Holz", 2, 0, 1), Queue("Holz", 2, 1, 1)], []);

        BuildingSnapshotReconciler.HasVanishedOtherPageEntries(snapshot, context.QueueBuildings.ToList()).ShouldBeFalse();
        BuildingSnapshotReconciler.Apply(context, new VillageId(1), [snapshot], snapshot);
        context.ChangeTracker.Clear();

        context.QueueBuildings.OrderBy(x => x.Position).Select(x => x.Location).ToArray().ShouldBe(new[] { 1, 2 });
    }

    [Fact]
    public void NameCache_IsIsolatedByAccountAndVillageAndReplacesPageNames()
    {
        var cache = new BuildingNameCache();
        cache.Store(new AccountId(1), new VillageId(1), Snapshot(1, [], [], [new("Holz", 1)]));
        cache.Store(new AccountId(1), new VillageId(1), Snapshot(2, [], [], [new("Kaserne", 19)]));
        cache.Store(new AccountId(2), new VillageId(1), Snapshot(1, [], [], [new("Wood", 1)]));
        cache.Store(new AccountId(1), new VillageId(2), Snapshot(1, [], [], [new("Bois", 1)]));
        cache.Store(new AccountId(1), new VillageId(1), Snapshot(1, [], [], [new("Holzfäller", 1)]));

        cache.Get(new AccountId(1), new VillageId(1)).Select(x => x.Name).Order().ShouldBe(new[] { "Holzfäller", "Kaserne" });
        cache.Get(new AccountId(2), new VillageId(1)).Single().Name.ShouldBe("Wood");
        cache.Get(new AccountId(1), new VillageId(2)).Single().Name.ShouldBe("Bois");
    }

    private static BuildingParser.BuildingDataDto Snapshot(int dorf, BuildingDto[] buildings, QueueBuildingDto[] queue, BuildingParser.BuildingNameDto[] names)
        => new(buildings, queue, names, dorf, 1);

    private static BuildingDto Building(int location, int type, int level)
        => new() { Location = location, Type = type, Level = level, IsUnderConstruction = true };

    private static QueueBuildingDto Queue(string name, int level, int position, int type = -1)
        => new() { Name = name, Level = level, Position = position, Type = type, DurationSeconds = 60, CompleteTime = Completion };

    private static QueueBuilding Existing(BuildingEnums type, int level) => new() { Type = type, Level = level };
}
