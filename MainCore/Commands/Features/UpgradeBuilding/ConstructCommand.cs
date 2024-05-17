namespace MainCore.Commands.Features.UpgradeBuilding
{
    public class ConstructCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, BuildingEnums building, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;

            var button = GetConstructButton(html, building);
            if (button is null) return Retry.ButtonNotFound("construct");

            var result = await chromeBrowser.Click(By.XPath(button.XPath), "dorf", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private static HtmlNode GetConstructButton(HtmlDocument doc, BuildingEnums building)
        {
            var node = doc.GetElementbyId($"contract_building{(int)building}");
            if (node is null) return null;

            var button = node
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("new"));

            return button;
        }
    }
}