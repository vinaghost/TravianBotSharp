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

        HtmlNode GetHeroAttributeNode(HtmlDocument doc);

        bool AttributeTabActive(HtmlDocument doc);

        HtmlNode GetFightingStrengthInputBox(HtmlDocument doc);

        HtmlNode GetOffBonusInputBox(HtmlDocument doc);

        HtmlNode GetResourceProductionInputBox(HtmlDocument doc);

        HtmlNode GetDefBonusInputBox(HtmlDocument doc);

        HtmlNode GetSaveButton(HtmlDocument doc);

        bool IsLevelUp(HtmlDocument doc);

        long[] GetRevivedResource(HtmlDocument doc);

        HtmlNode GetReviveButton(HtmlDocument doc);

        bool IsDead(HtmlDocument doc);

        int GetHelmet(HtmlDocument doc);

        int GetShoes(HtmlDocument doc);

        int GetBody(HtmlDocument doc);

        int GetLeftHand(HtmlDocument doc);

        int GetHorse(HtmlDocument doc);

        int GetRightHand(HtmlDocument doc);

        HeroDto Get(HtmlDocument doc);

        IEnumerable<AdventureDto> GetAdventures(HtmlDocument doc);
    }
}