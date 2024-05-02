using HtmlAgilityPack;
using MainCore.DTO;

namespace MainCore.Parsers
{
    public interface IFarmParser
    {
        HtmlNode GetStartAllButton(HtmlDocument doc);

        HtmlNode GetStartButton(HtmlDocument doc, FarmId raidId);

        IEnumerable<FarmDto> Get(HtmlDocument doc);
    }
}