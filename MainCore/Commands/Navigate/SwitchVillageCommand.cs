using MainCore.Commands.Abstract;

namespace MainCore.Commands.Navigate
{
    public class SwitchVillageCommand : VillagePanelCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, VillageId villageId, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var node = GetVillageNode(html, villageId);
            if (node is null) return Skip.VillageNotFound;

            if (IsActive(node)) return Result.Ok();

            var current = GetCurrentVillageId(html);

            bool villageChanged(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);

                var villageNode = GetVillageNode(doc, villageId);
                return villageNode is not null && IsActive(villageNode);
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath), villageChanged, cancellationToken);
            if (result.IsFailed) return result
                    .WithError(new Error($"page stuck at changing village stage [Current: {current}] [Expected: {villageId}]"))
                    .WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}