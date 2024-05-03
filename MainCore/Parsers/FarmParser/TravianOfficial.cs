using HtmlAgilityPack;
using MainCore.DTO;

namespace MainCore.Parsers.FarmParser
{
    [RegisterAsParser]
    public class TravianOfficial : IFarmParser
    {
        public HtmlNode GetStartButton(HtmlDocument doc, FarmId raidId)
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

        public HtmlNode GetStartAllButton(HtmlDocument doc)
        {
            var raidList = doc.GetElementbyId("rallyPointFarmList");
            if (raidList is null) return null;
            var startAll = raidList
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("startAllFarmLists"));
            return startAll;
        }

        public IEnumerable<FarmDto> Get(HtmlDocument doc)
        {
            var nodes = GetFarmNodes(doc);
            foreach (var node in nodes)
            {
                var id = GetId(node);
                var name = GetName(node);
                yield return new()
                {
                    Id = id,
                    Name = name,
                };
            }
        }

        private static IEnumerable<HtmlNode> GetFarmNodes(HtmlDocument doc)
        {
            var raidList = doc.GetElementbyId("rallyPointFarmList");
            if (raidList is null) return Enumerable.Empty<HtmlNode>();
            var fls = raidList
                .Descendants("div")
                .Where(x => x.HasClass("farmListHeader"));
            return fls;
        }

        private static string GetName(HtmlNode node)
        {
            var flName = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("name"));
            if (flName is null) return null;
            return flName.InnerText.Trim();
        }

        private static FarmId GetId(HtmlNode node)
        {
            var flId = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("dragAndDrop"));
            var id = flId.GetAttributeValue("data-list", "0");
            return new FarmId(id.ToInt());
        }
    }
}