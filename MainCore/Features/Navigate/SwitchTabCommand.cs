namespace MainCore.Features.Navigate;

[Handler]
public static partial class SwitchTabCommand
{
    public sealed record Command(IChromeBrowser Browser, int TabIndex) : ICommand
    {
        public string Name => $"{nameof(SwitchTabCommand)} [{Description}]";
        private string Description => $"#{TabIndex + 1}";
    }

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var (browser, tabIndex) = command;
        var tabs = browser.CurrentPage.GetTabs();

        var countTabs = await tabs.CountAsync();
        if (countTabs == 0) return Result.Ok();

        if (tabIndex >= countTabs) return Retry.Error.WithError($"Found {countTabs} tabs but need tab #{tabIndex + 1} active");

        var tab = tabs.Nth(tabIndex);
        if (await tab.IsTabActive()) return Result.Ok();

        Result result;
        result = await browser.Click(tab);
        if (result.IsFailed) return result;

        result = await browser.Wait(tab, condition: Parser.TabActiveExpression);
        if (result.IsFailed) return result;
        return Result.Ok();
    }
}