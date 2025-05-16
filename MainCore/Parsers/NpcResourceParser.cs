namespace MainCore.Parsers
{
    public static class NpcResourceParser
    {
        public static bool IsNpcDialog(HtmlDocument doc)
        {
            var dialog = doc.GetElementbyId("npc");
            return dialog is not null;
        }

        public static HtmlNode GetExchangeResourcesButton(HtmlDocument doc)
        {
            var npcMerchantDialog = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("npcMerchant"));

            BrokenParserException.ThrowIfNull(npcMerchantDialog);

            var exchangeResourceButton = npcMerchantDialog.Descendants("button")
                .FirstOrDefault(x => x.HasClass("gold"));
            BrokenParserException.ThrowIfNull(exchangeResourceButton);
            return exchangeResourceButton;
        }

        public static HtmlNode GetRedeemButton(HtmlDocument doc)
        {
            var button = doc.GetElementbyId("npc_market_button");
            return button;
        }

        public static long GetSum(HtmlDocument doc)
        {
            var sum = doc.GetElementbyId("sum");
            if (sum is null) return -1;
            return sum.InnerText.ParseLong();
        }

        public static IEnumerable<HtmlNode> GetInputs(HtmlDocument doc)
        {
            var wood = doc.DocumentNode.Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == "desired0");
            BrokenParserException.ThrowIfNull(wood);
            yield return wood;

            var clay = doc.DocumentNode.Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == "desired1");
            BrokenParserException.ThrowIfNull(clay);
            yield return clay;

            var iron = doc.DocumentNode.Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == "desired2");
            BrokenParserException.ThrowIfNull(iron);
            yield return iron;

            var crop = doc.DocumentNode.Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == "desired3");
            BrokenParserException.ThrowIfNull(crop);
            yield return crop;
        }
    }
}