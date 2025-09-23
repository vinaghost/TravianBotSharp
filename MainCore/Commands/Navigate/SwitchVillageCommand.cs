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

            var villageNode = VillagePanelParser.GetVillageNode(browser.Html, villageId);
            if (villageNode is null) return Skip.VillageNotFound;

            if (VillagePanelParser.IsActive(villageNode)) return Result.Ok();

            var (_, isFailed, element, errors) = await browser.GetElement(By.XPath(villageNode.XPath), cancellationToken);
            if (isFailed) return Result.Fail(errors);
            Result result;
            result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            bool villageChanged(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);

                var villageNode = VillagePanelParser.GetVillageNode(doc, villageId);
                return villageNode is not null && VillagePanelParser.IsActive(villageNode);
            }

            result = await browser.Wait(villageChanged, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}