namespace MainCore.Parsers
{
    public static class FarmListParser
    {
        public static IEnumerable<HtmlNode> GetFarmNodes(HtmlDocument doc)
        {
            var farmListTable = doc.GetElementbyId("rallyPointFarmList");

            BrokenParserException.ThrowIfNull(farmListTable);

            var farmlistNodes = farmListTable
                .Descendants("div")
                .Where(x => x.HasClass("farmListHeader"));
            return farmlistNodes;
        }

        public static FarmId GetId(HtmlNode node)
        {
            var farmlistDiv = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("dragAndDrop"));

            BrokenParserException.ThrowIfNull(farmlistDiv);

            var id = farmlistDiv.GetAttributeValue("data-list", "0");
            return new FarmId(id.ParseInt());
        }

        public static string GetName(HtmlNode node)
        {
            var farmlistName = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("name"));

            BrokenParserException.ThrowIfNull(farmlistName);
            return farmlistName.InnerText.Trim();
        }

        public static HtmlNode GetStartButton(HtmlDocument doc, FarmId raidId)
        {
            var nodes = GetFarmNodes(doc);
            foreach (var node in nodes)
            {
                var id = GetId(node);
                if (id != raidId) continue;

                var startNode = node
                    .Descendants("button")
                    .FirstOrDefault(x => x.HasClass("startFarmList"));
                if (startNode is null) continue;
                return startNode;
            }

            throw BrokenParserException.NotFound($"startFarmlistButton {raidId}");
        }

        public static HtmlNode GetStartAllButton(HtmlDocument doc)
        {
            var farmlistTable = doc.GetElementbyId("rallyPointFarmList");
            BrokenParserException.ThrowIfNull(farmlistTable);
            var startAllFarmListButton = farmlistTable
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("startAllFarmLists"));
            BrokenParserException.ThrowIfNull(startAllFarmListButton);
            return startAllFarmListButton;
        }
    }
}