using HtmlAgilityPack;

namespace MainCore.Parsers.BuildingParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : IBuildingParser
    {
        public HtmlNode GetBuilding(HtmlDocument doc, int location)
        {
            if (location < 19) return GetField(doc, location);
            return GetInfrastructure(doc, location);
        }

        private static HtmlNode GetField(HtmlDocument doc, int location)
        {
            return null;
        }

        private static HtmlNode GetInfrastructure(HtmlDocument doc, int location)
        {
            return null;
        }
    }
}