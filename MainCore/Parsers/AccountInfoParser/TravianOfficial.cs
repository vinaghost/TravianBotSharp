using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Common.Extensions;
using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;
using System.Net;

namespace MainCore.Parsers.AccountInfoParser
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : IAccountInfoParser
    {
        public AccountInfoDto Get(HtmlDocument doc)
        {
            var dto = new AccountInfoDto()
            {
                Gold = GetGold(doc),
                Silver = GetSilver(doc),
                HasPlusAccount = HasPlusAccount(doc),
                Tribe = TribeEnums.Any,
                MaximumVillage = GetMaximumVillage(doc),
            };

            return dto;
        }

        private static int GetGold(HtmlDocument doc)
        {
            var goldNode = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("ajaxReplaceableGoldAmount"));
            if (goldNode is null) return -1;
            var valueStrFixed = WebUtility.HtmlDecode(goldNode.InnerText);
            if (string.IsNullOrEmpty(valueStrFixed)) return -1;
            return valueStrFixed.ToInt();
        }

        private static int GetSilver(HtmlDocument doc)
        {
            var silverNode = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("ajaxReplaceableSilverAmount"));
            if (silverNode is null) return -1;
            var valueStrFixed = WebUtility.HtmlDecode(silverNode.InnerText);
            if (string.IsNullOrEmpty(valueStrFixed)) return -1;
            return valueStrFixed.ToInt();
        }

        private static bool HasPlusAccount(HtmlDocument doc)
        {
            var market = doc.DocumentNode.Descendants("a").FirstOrDefault(x => x.HasClass("market") && x.HasClass("round"));
            if (market is null) return false;

            if (market.HasClass("green")) return true;
            if (market.HasClass("gold")) return false;
            return false;
        }

        private static int GetMaximumVillage(HtmlDocument doc)
        {
            var sidebarBoxVillagelist = doc.GetElementbyId("sidebarBoxVillagelist");
            if (sidebarBoxVillagelist is null) return 0;

            var expansionSlotInfo = sidebarBoxVillagelist.Descendants("div").FirstOrDefault(x => x.HasClass("expansionSlotInfo"));
            if (expansionSlotInfo is null) return 0;

            var slots = expansionSlotInfo.Descendants("span").FirstOrDefault(x => x.HasClass("slots"));
            if (slots is null) return 0;
            var valueStrFixed = WebUtility.HtmlDecode(slots.InnerText);
            if (string.IsNullOrEmpty(valueStrFixed)) return -1;
            valueStrFixed = valueStrFixed.Split('/')[1];
            return valueStrFixed.ToInt();
        }
    }
}