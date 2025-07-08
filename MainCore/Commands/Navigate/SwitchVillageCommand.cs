namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class SwitchVillageCommand
    {
        public sealed record Command(VillageId VillageId) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IChromeBrowser browser,
           CancellationToken cancellationToken
           )
        {
            var villageId = command.VillageId;

            var node = VillagePanelParser.GetVillageNode(browser.Html, villageId);
            if (node is null) return Skip.VillageNotFound;

            if (VillagePanelParser.IsActive(node)) return Result.Ok();

            bool villageChanged(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);

                var villageNode = VillagePanelParser.GetVillageNode(doc, villageId);
                return villageNode is not null && VillagePanelParser.IsActive(villageNode);
            }

            Result result;
            result = await browser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result;

            result = await browser.Wait(villageChanged, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}