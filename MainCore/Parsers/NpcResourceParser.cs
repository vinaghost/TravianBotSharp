namespace MainCore.Parsers
{
    public static class NpcResourceParser
    {
        public static bool IsNpcDialog(HtmlDocument doc)
        {
            var dialog = doc.GetElementbyId("npc");
            return dialog is not null;
        }

        public static HtmlNode? GetExchangeResourcesButton(HtmlDocument doc)
        {
            var npcMerchantDialog = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("npcMerchant"));
            if (npcMerchantDialog is null) return null;

            var exchangeResourceButton = npcMerchantDialog.Descendants("button")
                .FirstOrDefault(x => x.HasClass("gold"));
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
            if (wood is null) throw new Exception("Wood input not found");
            yield return wood;

            var clay = doc.DocumentNode.Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == "desired1");
            if (clay is null) throw new Exception("Clay input not found");
            yield return clay;

            var iron = doc.DocumentNode.Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == "desired2");
            if (iron is null) throw new Exception("Iron input not found");
            yield return iron;

            var crop = doc.DocumentNode.Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == "desired3");
            if (crop is null) throw new Exception("Crop input not found");
            yield return crop;
        }

        public static HtmlNode? GetDistributeButton(HtmlDocument doc)
        {
            var submitText = doc.GetElementbyId("submitText");
            if (submitText is null) return null;
            if (submitText.GetAttributeValue("style", string.Empty).Contains("display: none")) return null;
            var button = submitText.Descendants("button").FirstOrDefault();
            return button;
        }

        public static HtmlNode? GetOkButton(HtmlDocument doc)
        {
            var button = doc.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("dialogButtonOk"));
            return button;
        }

        public static bool IsOkButtonVisible(HtmlDocument doc)
        {
            var button = GetOkButton(doc);
            if (button is null) return false;
            var style = button.ParentNode?.GetAttributeValue("style", string.Empty) ?? string.Empty;
            return !style.Contains("display: none");
        }
    }
}

