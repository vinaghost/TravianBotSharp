namespace MainCore.Commands.Navigate
{
    [Handler]
    public sealed partial class SwitchTabCommand(IChromeBrowser browser)
    {
        public sealed record Command(int TabIndex) : ICommand;

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var tabIndex = command.TabIndex;
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