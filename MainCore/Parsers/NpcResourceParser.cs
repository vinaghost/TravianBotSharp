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
            var npcMerchant = doc.DocumentNode.Descendants("div")
                .FirstOrDefault(x => x.HasClass("npcMerchant"));
            if (npcMerchant is null) return null;
            var button = npcMerchant.Descendants("button")
                .FirstOrDefault(x => x.HasClass("gold"));
            return button;
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
            for (int i = 0; i < 4; i++)
            {
                var node = doc.DocumentNode.Descendants("input")
                    .FirstOrDefault(x => x.GetAttributeValue("name", "") == $"desired{i}");
                yield return node;
            }
        }
    }
}