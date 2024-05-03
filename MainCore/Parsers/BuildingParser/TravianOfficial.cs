using HtmlAgilityPack;

namespace MainCore.Parsers.BuildingParser
{
    [RegisterAsParser]
    public class TravianOfficial : IBuildingParser
    {
        public HtmlNode GetBuilding(HtmlDocument doc, int location)
        {
            if (location < 19) return GetField(doc, location);
            return GetInfrastructure(doc, location);
        }

        private static HtmlNode GetField(HtmlDocument doc, int location)
        {
            var node = doc.DocumentNode
                   .Descendants("a")
                   .FirstOrDefault(x => x.HasClass($"buildingSlot{location}"));
            return node;
        }

        private static HtmlNode GetInfrastructure(HtmlDocument doc, int location)
        {
            var tmpLocation = location - 18;
            var div = doc.DocumentNode
                .SelectSingleNode($"//*[@id='villageContent']/div[{tmpLocation}]");

            return div;
        }
    }
}