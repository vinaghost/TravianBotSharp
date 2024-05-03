using HtmlAgilityPack;
using MainCore.DTO;

namespace MainCore.Parsers.FarmParser
{
    
    public class TTWars : IFarmParser
    {
        public HtmlNode GetStartButton(HtmlDocument doc, FarmId raidId)
        {
            var farmNode = doc.GetElementbyId($"raidList{raidId}");
            if (farmNode is null) return null;
            var startNode = farmNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("startButton"));
            return startNode;
        }

        public HtmlNode GetStartAllButton(HtmlDocument doc)
        {
            return null;
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

        private static List<HtmlNode> GetFarmNodes(HtmlDocument doc)
        {
            var raidList = doc.GetElementbyId("raidList");
            if (raidList is null) return new();
            var fls = raidList.Descendants("div").Where(x => x.HasClass("raidList"));

            return fls.ToList();
        }

        private static string GetName(HtmlNode node)
        {
            var flName = node.Descendants("div").FirstOrDefault(x => x.HasClass("listName"));
            if (flName is null) return null;
            var name = flName.Descendants("span").FirstOrDefault(x => x.HasClass("value"));
            if (name is null) return null;
            return name.InnerText.Trim();
        }

        private static FarmId GetId(HtmlNode node)
        {
            var id = node.Id;
            var value = new string(id.Where(c => char.IsDigit(c)).ToArray());
            return new FarmId(int.Parse(value));
        }
    }
}