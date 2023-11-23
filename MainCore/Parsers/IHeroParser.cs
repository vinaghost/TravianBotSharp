using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.DTO;

namespace MainCore.Parsers
{
    public interface IHeroParser
    {
        HtmlNode GetAmountBox(HtmlDocument doc);

        HtmlNode GetConfirmButton(HtmlDocument doc);

        HtmlNode GetHeroAvatar(HtmlDocument doc);

        HtmlNode GetItemSlot(HtmlDocument doc, HeroItemEnums type);

        bool InventoryTabActive(HtmlDocument doc);

        IEnumerable<HeroItemDto> Get(HtmlDocument doc);

        bool HeroInventoryLoading(HtmlDocument doc);
    }
}