using MainCore.Features.Update;
using MainCore.Infrasturecture;
using MainCore.Infrasturecture.Entities;
using MainCore.Shared.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using BuildingParser = MainCore.Features.Update.Parsers;

namespace MainCore.Test;

public class BuildingSnapshotReconcilerTests
{
    private static readonly DateTime Completion = new(2030, 1, 1, 12, 0, 0);

    [Fact]
    public void ResolveQueue_UsesBothPagesAndKeepsUnresolvedEntries()
    {
        var snapshot = Snapshot(2, [], [Queue("Holz", 2), Queue("Kaserne", 3), Queue("Unbekannt", 1)]);
        var names = new[] { new BuildingParser.BuildingNameDto("Holz", 1), new BuildingParser.BuildingNameDto("Kaserne", 19) };

        BuildingSnapshotReconciler.ResolveQueue(snapshot, names).ShouldBeFalse();
        snapshot.Queue.Select(x => x.Type).ShouldBe(new[] { 1, 19, -1 });
        snapshot.Queue.Length.ShouldBe(3);
    }

    [Fact]
    public void ResolveQueue_RejectsConflictingNamesAndInvalidTimers()
    {
        var snapshot = Snapshot(1, [], [Queue("Same", 2), Queue("Woodcutter", 2)]);
        snapshot.Queue[1].DurationSeconds = -1;
        var names = new[] { new BuildingParser.BuildingNameDto("Same", 1), new BuildingParser.BuildingNameDto("Same", 2) };

        BuildingSnapshotReconciler.ResolveQueue(snapshot, names).ShouldBeFalse();
        snapshot.Queue[0].Type.ShouldBe(-1);
        snapshot.Queue[1].Type.ShouldBe(1);
    }

    [Fact]
    public void IsValidPage_RejectsIncompleteOrMalformedResourceFields()
    {
        var buildings = Enumerable.Range(1, 18).Select(x => Building(x, 1, 0, false)).ToArray();
        var snapshot = Snapshot(1, buildings, []);
        BuildingSnapshotReconciler.IsValidPage(snapshot).ShouldBeTrue();
        BuildingSnapshotReconciler.IsValidPage(Snapshot(1, buildings[..17], [])).ShouldBeFalse();
        buildings[0].Level = -1;
        BuildingSnapshotReconciler.IsValidPage(snapshot).ShouldBeFalse();
    }

    [Fact]
    public void SameQueue_DetectsChangesInOrderAndLevel()
    {
        var first = Snapshot(1, [], [Queue("Woodcutter", 2), Queue("Barracks", 3)]);
        var second = Snapshot(2, [], [Queue("woodcutter", 2), Queue("Barracks", 3)]);
        BuildingSnapshotReconciler.SameQueue(first, second).ShouldBeTrue();
        second.Queue[0].Level = 3;
        BuildingSnapshotReconciler.SameQueue(first, second).ShouldBeFalse();
    }

    [Fact]
    public void Apply_RefreshesBothPagesAndPreservesQueueMultiplicity()
    {
        using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
        connection.Open();
        using var context = CreateContext(connection);
        context.QueueBuildings.Add(new QueueBuilding { VillageId = 1, Type = BuildingEnums.Woodcutter, Level = 2, Position = 3, Location = 1, CompleteTime = Completion.AddHours(-1) });
        context.SaveChanges();
        var resource = Snapshot(1, [Building(1, 1, 1, true), Building(2, 1, 1, true)], []);
        var village = Snapshot(2, [Building(19, 19, 2, true)],
            [Queue("Woodcutter", 2, 1, 0), Queue("Woodcutter", 2, 1, 1), Queue("Barracks", 3, 19, 2)]);

        BuildingSnapshotReconciler.Apply(context, new VillageId(1), [resource, village], village);
        context.ChangeTracker.Clear();
        var queue = context.QueueBuildings.OrderBy(x => x.Position).ToList();
        queue.Count.ShouldBe(3);
        queue.Select(x => x.Location).ShouldBe(new[] { 1, 2, 19 });
        queue.Select(x => x.CompleteTime).ShouldAllBe(x => x == Completion);
        context.Buildings.Count().ShouldBe(3);
    }

    [Fact]
    public void Apply_UsesStableAssignmentsForUnchangedDuplicateQueue()
    {
        using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
        connection.Open();
        using var context = CreateContext(connection);
        var snapshot = Snapshot(1, [Building(2, 1, 1, true), Building(1, 1, 1, true)],
            [Queue("Woodcutter", 2, 1, 0), Queue("Woodcutter", 2, 1, 1)]);

        BuildingSnapshotReconciler.Apply(context, new VillageId(1), [snapshot], snapshot);
        context.ChangeTracker.Clear();
        var ids = context.QueueBuildings.OrderBy(x => x.Position).Select(x => x.Id).ToArray();
        BuildingSnapshotReconciler.Apply(context, new VillageId(1), [snapshot], snapshot);
        context.ChangeTracker.Clear();

        var items = context.QueueBuildings.OrderBy(x => x.Position).ToArray();
        items.Select(x => x.Id).ShouldBe(ids);
        items.Select(x => x.Location).ShouldBe(new[] { 1, 2 });
    }

    [Fact]
    public void Apply_ReassignsRemainingDuplicateAfterObservedCompletion()
    {
        using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
        connection.Open();
        using var context = CreateContext(connection);
        var first = Snapshot(1, [Building(1, 1, 1, true), Building(2, 1, 1, true)],
            [Queue("Woodcutter", 2, 1, 0), Queue("Woodcutter", 2, 1, 1)]);
        BuildingSnapshotReconciler.Apply(context, new VillageId(1), [first], first);
        context.ChangeTracker.Clear();

        var second = Snapshot(1, [Building(1, 1, 1, true), Building(2, 1, 2, false)], [Queue("Woodcutter", 2, 1)]);
        BuildingSnapshotReconciler.Apply(context, new VillageId(1), [second], second);
        context.ChangeTracker.Clear();

        context.QueueBuildings.Single().Location.ShouldBe(1);
        context.Buildings.OrderBy(x => x.Location).Select(x => x.Level).ToArray().ShouldBe(new[] { 1, 2 });
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    public void Apply_LeavesLocationsUnknownWhenCandidateCountDiffers(int itemCount, int buildingCount)
    {
        using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
        connection.Open();
        using var context = CreateContext(connection);
        var buildings = Enumerable.Range(1, buildingCount).Select(x => Building(x, 1, 1, true)).ToArray();
        var queue = Enumerable.Range(0, itemCount).Select(x => Queue("Woodcutter", 2, 1, x)).ToArray();
        var snapshot = Snapshot(1, buildings, queue);

        BuildingSnapshotReconciler.Apply(context, new VillageId(1), [snapshot], snapshot);
        context.ChangeTracker.Clear();

        context.QueueBuildings.Count().ShouldBe(itemCount);
        context.QueueBuildings.Select(x => x.Location).ToArray().ShouldAllBe(x => x == -1);
    }

    [Fact]
    public void Apply_UsesObservedLevelInsteadOfInferringCompletionOrCancellation()
    {
        using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
        connection.Open();
        using var context = CreateContext(connection);
        context.Buildings.Add(new Building { VillageId = 1, Location = 1, Type = BuildingEnums.Woodcutter, Level = 4 });
        context.QueueBuildings.Add(new QueueBuilding { VillageId = 1, Location = 1, Type = BuildingEnums.Woodcutter, Level = 5, CompleteTime = Completion });
        context.SaveChanges();
        var snapshot = Snapshot(1, [Building(1, 1, 4, false)], []);

        BuildingSnapshotReconciler.Apply(context, new VillageId(1), [snapshot], snapshot);
        context.ChangeTracker.Clear();
        context.QueueBuildings.Count().ShouldBe(0);
        context.Buildings.Single().Level.ShouldBe(4);
    }

    [Fact]
    public void Apply_DoesNotAssumeSuccessiveLevelsBelongToSameSlot()
    {
        using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
        connection.Open();
        using var context = CreateContext(connection);
        var snapshot = Snapshot(1, [Building(1, 1, 1, true)],
            [Queue("Woodcutter", 2, 1), Queue("Woodcutter", 3, 1, 1)]);

        BuildingSnapshotReconciler.Apply(context, new VillageId(1), [snapshot], snapshot);
        context.ChangeTracker.Clear();

        context.QueueBuildings.OrderBy(x => x.Position).Select(x => x.Location).ToArray().ShouldBe(new[] { 1, -1 });
    }

    [Fact]
    public void Apply_DoesNotAssignOneEmptySiteToDifferentQueueEntries()
    {
        using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
        connection.Open();
        using var context = CreateContext(connection);
        var snapshot = Snapshot(2, [Building(19, 0, 0, true)],
            [Queue("Barracks", 1, 19), Queue("Warehouse", 1, 10, 1)]);

        BuildingSnapshotReconciler.Apply(context, new VillageId(1), [snapshot], snapshot);
        context.ChangeTracker.Clear();
        context.QueueBuildings.OrderBy(x => x.Position).Select(x => x.Location).ToArray().ShouldBe(new[] { -1, -1 });
    }

    [Fact]
    public void Apply_DoesNotAssignDuplicateGroupWhenAnotherTypeCanUseItsSites()
    {
        using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
        connection.Open();
        using var context = CreateContext(connection);
        var snapshot = Snapshot(2, [Building(19, 0, 0, true), Building(20, 0, 0, true)],
            [Queue("Barracks", 1, 19), Queue("Barracks", 1, 19, 1), Queue("Warehouse", 1, 10, 2)]);

        BuildingSnapshotReconciler.Apply(context, new VillageId(1), [snapshot], snapshot);
        context.ChangeTracker.Clear();

        context.QueueBuildings.OrderBy(x => x.Position).Select(x => x.Location).ToArray().ShouldBe(new[] { -1, -1, -1 });
    }

    private static AppDbContext CreateContext(SqliteConnection connection)
    {
        var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        context.Database.EnsureCreated();
        return context;
    }

    private static BuildingParser.BuildingDataDto Snapshot(int dorf, BuildingDto[] buildings, QueueBuildingDto[] queue)
        => new(buildings, queue, [], dorf, 1);

    private static BuildingDto Building(int location, int type, int level, bool active)
        => new() { Location = location, Type = type, Level = level, IsUnderConstruction = active };

    private static QueueBuildingDto Queue(string name, int level, int type = -1, int position = 0)
        => new() { Name = name, Type = type, Level = level, Position = position, DurationSeconds = 60, CompleteTime = Completion };
}
