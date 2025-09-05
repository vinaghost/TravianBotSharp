namespace MainCore.Parsers
{
    public static class FarmListParser
    {
        public static IEnumerable<HtmlNode> GetFarmNodes(HtmlDocument doc)
        {
            var farmListTable = doc.GetElementbyId("rallyPointFarmList");
            if (farmListTable is null) return [];

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

            if (farmlistDiv is null) return default;

            var id = farmlistDiv.GetAttributeValue("data-list", "0");
            return new FarmId(id.ParseInt());
        }

        public static string GetName(HtmlNode node)
        {
            var farmlistName = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("name"));
            if (farmlistName is null) return "";
            return farmlistName.InnerText.Trim();
        }

        public static HtmlNode? GetStartButton(HtmlDocument doc, FarmId raidId)
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
            return null;
        }

        public static HtmlNode? GetStartAllButton(HtmlDocument doc)
        {
            var farmlistTable = doc.GetElementbyId("rallyPointFarmList");
            if (farmlistTable is null) return null;
            var startAllFarmListButton = farmlistTable
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("startAllFarmLists"));

            return startAllFarmListButton;
        }
    }
}
