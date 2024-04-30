using HtmlAgilityPack;
using MainCore.DTO;
using System.Net;

namespace MainCore.Parsers.AccountInfoParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : IAccountInfoParser
    {
        public AccountInfoDto Get(HtmlDocument doc)
        {
            var dto = new AccountInfoDto()
            {
                Gold = GetGold(doc),
                Silver = GetSilver(doc),
                HasPlusAccount = HasPlusAccount(doc),
                Tribe = TribeEnums.Any,
            };
            return dto;
        }

        private static int GetGold(HtmlDocument doc)
        {
            var goldNode = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("ajaxReplaceableGoldAmount"));
            if (goldNode is null) return -1;
            var valueStrFixed = WebUtility.HtmlDecode(goldNode.InnerText);
            if (string.IsNullOrEmpty(valueStrFixed)) return -1;
            var valueStr = new string(valueStrFixed.Where(c => char.IsDigit(c)).ToArray());
            if (string.IsNullOrEmpty(valueStr)) return -1;
            return int.Parse(valueStr);
        }

        private static int GetSilver(HtmlDocument doc)
        {
            var silverNode = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("ajaxReplaceableSilverAmount"));
            if (silverNode is null) return -1;
            var valueStrFixed = WebUtility.HtmlDecode(silverNode.InnerText);
            if (string.IsNullOrEmpty(valueStrFixed)) return -1;
            var valueStr = new string(valueStrFixed.Where(c => char.IsDigit(c)).ToArray());
            if (string.IsNullOrEmpty(valueStr)) return -1;
            return int.Parse(valueStr);
        }

        private static bool HasPlusAccount(HtmlDocument doc)
        {
            var barracksButton = doc.DocumentNode.Descendants("a").FirstOrDefault(x => x.HasClass("barracks") && x.HasClass("round"));
            if (barracksButton is null) return false;
            return barracksButton.HasClass("green");
        }
    }
}