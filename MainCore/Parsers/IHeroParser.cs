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

        IEnumerable<HeroItemDto> GetItems(HtmlDocument doc);

        bool HeroInventoryLoading(HtmlDocument doc);

        HtmlNode GetHeroAdventure(HtmlDocument doc);

        bool CanStartAdventure(HtmlDocument doc);

        HtmlNode GetAdventure(HtmlDocument doc);

        string GetAdventureInfo(HtmlNode node);
        HtmlNode GetContinueButton(HtmlDocument doc);
        TimeSpan GetAdventureDuration(HtmlDocument doc);
    }
}