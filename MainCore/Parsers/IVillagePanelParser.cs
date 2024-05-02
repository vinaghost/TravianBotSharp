using HtmlAgilityPack;
using MainCore.DTO;

namespace MainCore.Parsers
{
    public interface IVillagePanelParser
    {
        HtmlNode GetVillageNode(HtmlDocument doc, VillageId villageId);

        bool IsActive(HtmlNode node);

        IEnumerable<VillageDto> Get(HtmlDocument doc);

        VillageId GetCurrentVillageId(HtmlDocument doc);
    }
}