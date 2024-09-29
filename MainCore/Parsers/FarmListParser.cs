namespace MainCore.Parsers
{
    public static class FarmListParser
    {
        public static IEnumerable<HtmlNode> GetFarmNodes(HtmlDocument doc)
        {
            var raidList = doc.GetElementbyId("rallyPointFarmList");
            if (raidList is null) return Enumerable.Empty<HtmlNode>();
            var fls = raidList
                .Descendants("div")
                .Where(x => x.HasClass("farmListHeader"));
            return fls;
        }

        public static FarmId GetId(HtmlNode node)
        {
            var flId = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("dragAndDrop"));
            if (flId is null) return FarmId.Empty;
            var id = flId.GetAttributeValue("data-list", "0");
            return new FarmId(id.ParseInt());
        }

        public static string GetName(HtmlNode node)
        {
            var flName = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("name"));
            if (flName is null) return null;
            return flName.InnerText.Trim();
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
                return startNode;
            }
            return null;
        }

        public static HtmlNode GetStartAllButton(HtmlDocument doc)
        {
            var raidList = doc.GetElementbyId("rallyPointFarmList");
            if (raidList is null) return null;
            var startAll = raidList
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("startAllFarmLists"));
            return startAll;
        }
    }
}