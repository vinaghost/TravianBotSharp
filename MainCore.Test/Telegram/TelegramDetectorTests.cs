using MainCore.Entities;
using MainCore.Enums;
using MainCore.Notifications;
using MainCore.Parsers;
using MainCore.Services.Telegram;
using MainCore.Test.Parsers;

namespace MainCore.Test.Telegram;

public class TelegramDetectorTests
{
    [Fact]
    public void AttackAlertsFireOncePerObservedAttackPeriodAndAreIsolatedByAccount()
    {
        var detector = new TelegramDetector();
        var scan = new TelegramVillageScan(new(1), new(2), UnderAttack: true);
        detector.Observe(scan).Single().Kind.ShouldBe(TelegramEvents.IncomingAttack);
        detector.Observe(scan).ShouldBeEmpty();
        detector.Observe(scan with { AccountId = new(3) }).Count().ShouldBe(1);
        detector.Observe(scan with { UnderAttack = false }).ShouldBeEmpty();
        detector.Observe(scan).Count().ShouldBe(1);
    }

    [Fact]
    public void BuildingsNeedABaselineAndPartialPageRefreshesPreserveOtherSlots()
    {
        var detector = new TelegramDetector();
        TelegramVillageScan Scan(params ObservedBuilding[] buildings) => new(new(1), new(1), Buildings: buildings);
        detector.Observe(Scan(new(1, BuildingEnums.Woodcutter, 1), new(21, BuildingEnums.Site, 0))).ShouldBeEmpty();
        detector.Observe(Scan(new ObservedBuilding(2, BuildingEnums.ClayPit, 4))).ShouldBeEmpty();
        var changes = detector.Observe(Scan(new(1, BuildingEnums.Woodcutter, 2), new(21, BuildingEnums.Barracks, 1))).Single();
        changes.Kind.ShouldBe(TelegramEvents.BuildingCompleted);
        changes.Text.ShouldContain("Woodcutter: level 2");
        changes.Text.ShouldContain("Barracks: level 1");
        detector.Observe(Scan(new ObservedBuilding(1, BuildingEnums.Woodcutter, 2))).ShouldBeEmpty();
        detector.Observe(Scan(new ObservedBuilding(1, BuildingEnums.Woodcutter, 1))).ShouldBeEmpty(); // Demolition is not completion.
    }

    [Fact]
    public void ResourceThresholdsUseHysteresisAndRejectMissingCapacity()
    {
        var detector = new TelegramDetector();
        TelegramVillageScan Scan(long wood, long crop, long capacity = 1000) => new(new(1), new(2), Storage: new(wood, 500, 500, crop, capacity, capacity));
        detector.Observe(Scan(900, 100, 0)).ShouldBeEmpty();
        detector.Observe(Scan(900, 100)).Select(x => x.Kind).ShouldBe(new[] { TelegramEvents.StorageFull, TelegramEvents.LowCrop });
        detector.Observe(Scan(890, 110)).ShouldBeEmpty();
        detector.Observe(Scan(910, 90)).ShouldBeEmpty();
        detector.Observe(Scan(800, 200)).ShouldBeEmpty();
        detector.Observe(Scan(900, 100)).Count().ShouldBe(2);
    }

    [Fact]
    public void StatusAlertsSkipTransitionsDuplicatesAndInitialOffline()
    {
        var detector = new TelegramDetector();
        detector.Status(new(new(1), StatusEnums.Offline)).ShouldBeNull();
        detector.Status(new(new(1), StatusEnums.Starting)).ShouldBeNull();
        detector.Status(new(new(1), StatusEnums.Online)).ShouldNotBeNull();
        detector.Status(new(new(1), StatusEnums.Online)).ShouldBeNull();
        detector.Status(new(new(1), StatusEnums.Pausing)).ShouldBeNull();
        detector.Status(new(new(1), StatusEnums.Paused)).ShouldNotBeNull();
    }

    [Fact]
    public void EnabledSettingsRequireARecipientAndAtLeastOneCheckbox()
    {
        new TelegramOptions().Validate();
        Should.Throw<InvalidOperationException>(() => new TelegramOptions { Enabled = true }.Validate());
        Should.Throw<InvalidOperationException>(() => new TelegramOptions { Enabled = true, ChatId = "-1001234567890", Events = TelegramEvents.None }.Validate());
        new TelegramOptions { Enabled = true, ChatId = "-1001234567890" }.Validate();
        TelegramOptions.ValidateChat("-1001234567890").ShouldBe(-1001234567890L);
    }
}

public class TelegramVillageParserTests(PlaywrightFixture fixture) : ParserTestBase(fixture)
{
    [Theory]
    [InlineData("active attack", true, true)]
    [InlineData("active", true, false)]
    [InlineData("attack", false, true)]
    [InlineData("", false, false)]
    public async Task PreservesAttackAndActiveFlagsFromTheGame(string classes, bool active, bool attack)
    {
        await using var context = await CreateContextAsync();
        var page = await context.NewPageAsync();
        await page.SetContentAsync($"<div id='sidebarBoxVillageList'><div class='listEntry village {classes}' data-did='42'><a><span class='name'>Village</span></a><span class='coordinateX'>12</span><span class='coordinateY'>-5</span></div></div>");
        var village = (await VillagePanelParser.Get(page)).Single();
        village.Id.ShouldBe(new VillageId(42));
        village.IsActive.ShouldBe(active);
        village.IsUnderAttack.ShouldBe(attack);
    }
}
