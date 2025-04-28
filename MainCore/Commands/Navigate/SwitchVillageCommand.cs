using MainCore.Commands.Abstract;

namespace MainCore.Commands.Navigate
{
    [RegisterScoped<SwitchVillageCommand>]
    public class SwitchVillageCommand(IDataService dataService) : CommandBase(dataService), ICommand
    {
        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var villageId = _dataService.VillageId;
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var node = VillagePanelParser.GetVillageNode(html, villageId);
            if (node is null) return Skip.VillageNotFound;

            if (VillagePanelParser.IsActive(node)) return Result.Ok();

            var current = VillagePanelParser.GetCurrentVillageId(html);

            bool villageChanged(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);

                var villageNode = VillagePanelParser.GetVillageNode(doc, villageId);
                return villageNode is not null && VillagePanelParser.IsActive(villageNode);
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result
                    .WithError(new Error($"page stuck at changing village stage [Current: {current}] [Expected: {villageId}]"))
                    .WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.Wait(villageChanged, cancellationToken);
            if (result.IsFailed) return result
                   .WithError(new Error($"page stuck at changing village stage [Current: {current}] [Expected: {villageId}]"))
                   .WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}