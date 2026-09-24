namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class SwitchTabCommand
    {
        public sealed record Command(int TabIndex) : ICommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IChromeBrowser browser,
           CancellationToken cancellationToken
           )
        {
            return await SwitchTab(browser, command.TabIndex, cancellationToken);
        }

        public static async ValueTask<Result> SwitchTab(
            IChromeBrowser browser,
            int tabIndex,
            CancellationToken cancellationToken)
        {
            var tabs = BuildingTabParser.GetTabs(browser.CurrentPage);

            var countTabs = await tabs.CountAsync();
            if (countTabs == 0) return Result.Ok();

            if (tabIndex >= countTabs) return Retry.Error.WithError($"Found {countTabs} tabs but need tab #{tabIndex + 1} active");

            var tab = tabs.Nth(tabIndex);
            if (await BuildingTabParser.IsTabActive(tab)) return Result.Ok();

            Result result;
            result = await browser.Click(tab);
            if (result.IsFailed) return result;

            result = await browser.Wait(tab, condition: "node => node.classList.contains('active')");
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}