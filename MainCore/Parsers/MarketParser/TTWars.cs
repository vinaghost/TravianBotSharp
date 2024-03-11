using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Common.Extensions;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.MarketParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : IMarketParser
    {
        public bool NPCDialogShown(HtmlDocument doc)
        {
            var dialog = doc.GetElementbyId("npc");
            return dialog is not null;
        }

        public HtmlNode GetExchangeResourcesButton(HtmlDocument doc)
        {
            var npcMerchant = doc.DocumentNode.Descendants("div")
                .FirstOrDefault(x => x.HasClass("npcMerchant"));
            if (npcMerchant is null) return null;
            var button = npcMerchant.Descendants("button")
                .FirstOrDefault(x => x.HasClass("gold"));
            return button;
        }

        public HtmlNode GetDistributeButton(HtmlDocument doc)
        {
            var submitText = doc.GetElementbyId("submitText");
            if (submitText is null) return null;
            var button = submitText.Descendants("button")
                .FirstOrDefault();
            return button;
        }

        public HtmlNode GetRedeemButton(HtmlDocument doc)
        {
            var button = doc.GetElementbyId("npc_market_button");
            return button;
        }

        public long GetSum(HtmlDocument doc)
        {
            var sum = doc.GetElementbyId("sum");
            if (sum is null) return -1;
            return sum.InnerText.ToLong();
        }

        public IEnumerable<HtmlNode> GetInputs(HtmlDocument doc)
        {
            for (int i = 0; i < 4; i++)
            {
                var node = doc.DocumentNode.Descendants("input")
                    .Where(x => x.GetAttributeValue("name", "") == $"desired{i}")
                    .FirstOrDefault();
                yield return node;
            }
        }
    }
}