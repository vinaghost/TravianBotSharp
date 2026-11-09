using MainCore.Features.Navigate;

namespace MainCore.Features.SetHeroPoint;

[Handler]
public sealed partial class OpenAttributesPanelCommand(SwitchTabCommand.Handler switchTabCommand)
{
    public sealed record Command(IChromeBrowser Browser) : ICommand
    {
        public string Name => nameof(OpenAttributesPanelCommand).Humanize();
    }

    private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var browser = command.Browser;
        Result result;

        var heroAvatar = browser.CurrentPage.GetHeroAvatar();
        result = await browser.Click(heroAvatar);
        if (result.IsFailed) return result;

        var heroPageLocator = browser.CurrentPage.GetHeroPanel();
        result = await browser.Wait(heroPageLocator);
        if (result.IsFailed) return result;

        result = await switchTabCommand.HandleAsync(new(browser, 1), cancellationToken);
        if (result.IsFailed) return result;

        var attributeBoxes = browser.CurrentPage.GetHeroAttributeBoxes();
        result = await browser.Wait(attributeBoxes);
        if (result.IsFailed) return result;
        return Result.Ok();
    }
}