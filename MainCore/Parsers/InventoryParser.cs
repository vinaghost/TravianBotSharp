namespace MainCore.Parsers
{
    public static class InventoryParser
    {
        public static bool IsInventoryPage(HtmlDocument doc)
        {
            var heroDiv = doc.GetElementbyId("heroV2");
            if (heroDiv is null) return false;
            var aNode = heroDiv.Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("data-tab", 0) == 1);
            if (aNode is null) return false;
            return aNode.HasClass("active");
        }

        public static HtmlNode GetHeroAvatar(HtmlDocument doc)
        {
            return doc.GetElementbyId("heroImageButton");
        }

        public static bool IsInventoryLoaded(HtmlDocument doc)
        {
            var inventoryPageWrapper = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("inventoryPageWrapper"));
            if (inventoryPageWrapper is null) return false;
            return !inventoryPageWrapper.HasClass("loading");
        }

        public static HtmlNode? GetItemSlot(HtmlDocument doc, HeroItemEnums type)
        {
            var heroItemsDiv = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("heroItems"));

            if (heroItemsDiv is null) return null;

            var heroItemDivs = heroItemsDiv
                .Descendants("div")
                .Where(x => x.HasClass("heroItem") && !x.HasClass("empty"));

            if (!heroItemDivs.Any()) return null;

            foreach (var itemSlot in heroItemDivs)
            {
                if (itemSlot.ChildNodes.Count < 2) continue;
                var itemNode = itemSlot.ChildNodes[1];
                var classes = itemNode.GetClasses();
                if (classes.Count() < 2) continue;

                var itemValue = classes.ElementAt(1);

                if (itemValue.ParseInt() == (int)type) return itemSlot;
            }

            return null;
        }

        public static HtmlNode? GetAmountBox(HtmlDocument doc)
        {
            var dialogHeroItemConsumable = doc.GetElementbyId("consumableHeroItem");
            if (dialogHeroItemConsumable is null) return null;

            var amountInput = dialogHeroItemConsumable
                .Descendants("input")
                .FirstOrDefault();
            return amountInput;
        }

        public static HtmlNode? GetConfirmButton(HtmlDocument doc)
        {
            var dialog = doc.GetElementbyId("dialogContent");
            if (dialog is null) return null;

            var buttonWrapper = dialog
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("buttonsWrapper"));
            if (buttonWrapper is null) return null;

            var buttonTransfer = buttonWrapper.Descendants("button");
            if (buttonTransfer.Count() < 2) return null;
            return buttonTransfer.ElementAt(1);
        }
    }
}