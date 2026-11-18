using MainCore.Features.Update;
using MainCore.Shared.Enums;

namespace MainCore.Test;

public class BuildingParserTests(PlaywrightFixture fixture) : IClassFixture<PlaywrightFixture>
{
    [Fact]
    public async Task GetBuildingData_KeepsCrossPageAndUnresolvedQueueEntries()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.SetContentAsync("""
            <div class="village2" id="villageContent">
              <div class="buildingSlot" data-aid="19" data-gid="19" data-name="Kaserne">
                <a data-level="2" class="underConstruction"></a>
              </div>
            </div>
            <ul class="buildingList">
              <li><span class="name">Holz</span><span class="lvl">2</span><span class="timer" value="120"></span></li>
              <li><span class="name">Kaserne</span><span class="lvl">3</span><span class="timer" value="240"></span></li>
            </ul>
            """);

        var snapshot = await page.GetBuildingData();

        snapshot.Dorf.ShouldBe(2);
        snapshot.Buildings.Single().Location.ShouldBe(19);
        snapshot.Buildings.Single().IsUnderConstruction.ShouldBeTrue();
        snapshot.Queue.Select(x => x.Name).ShouldBe(new[] { "Holz", "Kaserne" });
        snapshot.Queue.Select(x => x.Position).ShouldBe(new[] { 0, 1 });
        snapshot.Queue.Select(x => x.DurationSeconds).ShouldBe(new[] { 120, 240 });
        BuildingSnapshotReconciler.ResolveQueue(snapshot, snapshot.Names).ShouldBeFalse();
        snapshot.Queue[1].Type.ShouldBe((int)BuildingEnums.Barracks);
    }

    [Fact]
    public async Task GetBuildingData_ReadsResourceNamesAndRejectsMissingTimer()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.SetContentAsync("""
            <div class="village1"><div id="resourceFieldContainer">
              <a class="level buildingSlot1 gid1 level1 underConstruction" title="<span class='title'>Holz</span>"></a>
            </div></div>
            <ul class="buildingList"><li><span class="name">Holz</span><span class="lvl">2</span></li></ul>
            """);

        var snapshot = await page.GetBuildingData();

        snapshot.Dorf.ShouldBe(1);
        snapshot.Buildings.Single().Type.ShouldBe((int)BuildingEnums.Woodcutter);
        snapshot.Names.Single().Name.ShouldBe("Holz");
        snapshot.Queue.Single().DurationSeconds.ShouldBe(-1);
        BuildingSnapshotReconciler.ResolveQueue(snapshot, snapshot.Names).ShouldBeFalse();
    }
}
