namespace MainCore.Features.UpgradeBuilding;

[Handler]
public partial class UpgradeCommand(ILogger logger)
{
    public sealed record Command(IChromeBrowser Browser, NormalBuildPlan Plan, bool CanUseSpecialUpgrade) : ICommand
    {
        public string Name => $"{nameof(UpgradeCommand).Humanize()} [{Description}]";
        private string Description => $"{Plan}";
    }

    private async ValueTask<Result> HandleAsync(Command command)
    {
        var (browser, plan, canUseSpecialUpgrade) = command;

        Result result;
        if (canUseSpecialUpgrade && !UnskippableBuildings.Contains(plan.Type))
        {
            result = await SpecialUpgrade(browser, plan.Type);
            if (result.IsFailed) return result;
        }
        else
        {
            result = await Upgrade(browser, plan.Type);
            if (result.IsFailed) return result;
        }

        return Result.Ok();
    }

    private static readonly List<BuildingEnums> UnskippableBuildings =
    [
        BuildingEnums.Residence,
        BuildingEnums.Palace,
        BuildingEnums.CommandCenter,
    ];

    private async Task<Result> SpecialUpgrade(IChromeBrowser browser, BuildingEnums building, bool repeat = false)
    {
        var button = await browser.CurrentPage.GetSpecialButton(building);
        var result = await browser.Click(button);
        if (result.IsFailed) return result;

        var videoFeature = browser.CurrentPage.Locator("#videoFeature");

        await Task.Delay(5_000);
        if (await videoFeature.CountAsync() == 0)
        {
            browser.Logger.Information("Video not shown. Refreshing the page.");
            result = await browser.Refresh();
            if (result.IsFailed) return result;

            result = await browser.Click(button);
            if (result.IsFailed) return result;
        }

        result = await browser.Wait(videoFeature);
        if (result.IsFailed) return result;

        result = await HandleAds(browser, building, repeat);
        if (result.IsFailed) return result;
        return Result.Ok();
    }

    private async Task<Result> HandleAds(IChromeBrowser browser, BuildingEnums building, bool repeat = false)
    {
        if (repeat)
        {
            logger.Warning("Special upgrade failed after retrying.");
            return Retry.Error;
        }

        var page = browser.CurrentPage;
        var videoFeature = page.Locator("#videoFeature");
        Result result;
        var classess = await videoFeature.GetAttributeAsync("class") ?? "";
        if (classess.Contains("infoScreen"))
        {
            var checkBoxDontShowAgain = page.Locator("#videoFeature label.checkbox input[name=\"preference\"]");
            result = await browser.Click(checkBoxDontShowAgain);
            if (result.IsFailed) return result;

            var buttonWatchAds = page.Locator("#videoFeature div.buttonWrapper button");
            result = await browser.Click(buttonWatchAds);
            if (result.IsFailed) return result;
        }

        await Task.Delay(10_000);

        var videoAreaChildrend = browser.CurrentPage.Locator("#videoArea > *");
        if (await videoAreaChildrend.CountAsync() == 0)
        {
            browser.Logger.Information("Video not loaded. Refreshing the page.");
            result = await browser.Refresh();
            if (result.IsFailed) return result;
            result = await SpecialUpgrade(browser, building, true);
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        result = await browser.Wait(browser.CurrentPage.GetDorf(building));
        if (result.IsFailed) return result;
        return Result.Ok();
    }

    private static async Task<Result> Upgrade(IChromeBrowser browser, BuildingEnums building)
    {
        var button = await browser.CurrentPage.GetNormalButton(building);
        var result = await browser.Click(button);
        if (result.IsFailed) return result;

        result = await browser.Wait(browser.CurrentPage.GetDorf(building));
        if (result.IsFailed) return result;

        return Result.Ok();
    }
}