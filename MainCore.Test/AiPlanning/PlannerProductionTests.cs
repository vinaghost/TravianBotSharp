using MainCore.Services.AiPlanning;
using MainCore.Test.Parsers;

namespace MainCore.Test.AiPlanning
{
    public class PlannerProductionTests(PlaywrightFixture fixture) : ParserTestBase(fixture)
    {
        [Theory]
        [InlineData("active attack", true)]
        [InlineData("active", false)]
        public async Task ReadsIncomingAttackFromTheSelectedLiveVillage(string classes, bool expected)
        {
            await using var context = await CreateContextAsync();
            var page = await context.NewPageAsync();
            await page.SetContentAsync($"<div id='sidebarBoxVillageList'><div class='listEntry village {classes}' data-did='42'></div></div>");
            (await PlannerSnapshotService.ReadAttackStatusAsync(page, new(42))).ShouldBe(expected);
            await Should.ThrowAsync<InvalidOperationException>(() => PlannerSnapshotService.ReadAttackStatusAsync(page, new(99)));
        }

        [Fact]
        public async Task RejectsARefreshIfTheBrowserSwitchedAwayFromTheSelectedVillage()
        {
            await using var context = await CreateContextAsync();
            var page = await context.NewPageAsync();
            await page.SetContentAsync("<div id='sidebarBoxVillageList'><div class='listEntry village' data-did='42'></div></div>");
            await Should.ThrowAsync<InvalidOperationException>(() => PlannerSnapshotService.ReadAttackStatusAsync(page, new(42)));
        }

        [Fact]
        public async Task ReadsLiveHourlyProductionIncludingNegativeCropAndSeparators()
        {
            await using var context = await CreateContextAsync();
            var page = await context.NewPageAsync();
            await page.SetContentAsync("""
                <table id="production"><tr><th>Production per hour</th></tr>
                <tr><td>Wood</td><td class="num">1,234</td></tr>
                <tr><td>Clay</td><td class="num">2&#8239;345</td></tr>
                <tr><td>Iron</td><td class="num">3.456</td></tr>
                <tr><td>Crop</td><td class="num">&#8722;120</td></tr></table>
                """);
            (await PlannerSnapshotService.ReadProductionAsync(page)).ShouldBe(new long[] { 1234, 2345, 3456, -120 });
        }

        [Theory]
        [InlineData("<table id='production'><tr><td class='num'>10</td></tr></table>")]
        [InlineData("<table id='production'><tr><td class='num'>?</td></tr><tr><td class='num'>1</td></tr><tr><td class='num'>2</td></tr><tr><td class='num'>3</td></tr></table>")]
        public async Task MissingOrUnreadableProductionRemainsUnknown(string html)
        {
            await using var context = await CreateContextAsync();
            var page = await context.NewPageAsync();
            await page.SetContentAsync(html);
            (await PlannerSnapshotService.ReadProductionAsync(page)).ShouldBeNull();
        }
    }
}
