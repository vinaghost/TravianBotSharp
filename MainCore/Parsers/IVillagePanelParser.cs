using HtmlAgilityPack;
using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Parsers
{
    public interface IVillagePanelParser
    {
        HtmlNode GetVillageNode(HtmlDocument doc, VillageId villageId);

        bool IsActive(HtmlNode node);

        IEnumerable<VillageDto> Get(HtmlDocument doc);
    }
}