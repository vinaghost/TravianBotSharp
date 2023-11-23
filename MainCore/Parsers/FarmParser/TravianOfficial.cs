using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.FarmParser
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : IFarmParser
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
            var raidList = doc.GetElementbyId("raidList");
            if (raidList is null) return null;
            var startAll = raidList
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("startAll"));
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
            var raidList = doc.GetElementbyId("raidList");
            if (raidList is null) return Enumerable.Empty<HtmlNode>();
            var fls = raidList
                .Descendants("div")
                .Where(x => x.HasClass("raidList"));
            return fls;
        }

        private static string GetName(HtmlNode node)
        {
            var flName = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("listName"));
            if (flName is null) return null;
            return flName.InnerText.Trim();
        }

        private static FarmId GetId(HtmlNode node)
        {
            var id = node.GetAttributeValue("data-listid", "0");
            var value = new string(id.Where(c => char.IsDigit(c)).ToArray());
            return new FarmId(int.Parse(value));
        }
    }
}