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

        public static HtmlNode GetItemSlot(HtmlDocument doc, HeroItemEnums type)
        {
            var heroItemsDiv = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("heroItems"));
            BrokenParserException.ThrowIfNull(heroItemsDiv);

            var heroItemDivs = heroItemsDiv
                .Descendants("div")
                .Where(x => x.HasClass("heroItem") && !x.HasClass("empty"));
            BrokenParserException.ThrowIfEmpty(heroItemDivs);

            foreach (var itemSlot in heroItemDivs)
            {
                if (itemSlot.ChildNodes.Count < 2) continue;
                var itemNode = itemSlot.ChildNodes[1];
                var classes = itemNode.GetClasses();
                if (classes.Count() != 2) continue;

                var itemValue = classes.ElementAt(1);

                if (itemValue.ParseInt() == (int)type) return itemSlot;
            }
            throw BrokenParserException.NotFound("itemSlot");
        }

        public static HtmlNode GetAmountBox(HtmlDocument doc)
        {
            var dialogHeroItemConsumable = doc.GetElementbyId("consumableHeroItem");
            BrokenParserException.ThrowIfNull(dialogHeroItemConsumable);

            var amountInput = dialogHeroItemConsumable
                .Descendants("input")
                .FirstOrDefault();

            BrokenParserException.ThrowIfNull(amountInput);
            return amountInput;
        }

        public static HtmlNode GetConfirmButton(HtmlDocument doc)
        {
            var dialog = doc.GetElementbyId("dialogContent");

            BrokenParserException.ThrowIfNull(dialog);

            var buttonWrapper = dialog
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("buttonsWrapper"));

            BrokenParserException.ThrowIfNull(buttonWrapper);

            var buttonTransfer = buttonWrapper.Descendants("button");
            if (buttonTransfer.Count() < 2) throw BrokenParserException.NotFound("buttonTransfer");
            return buttonTransfer.ElementAt(1);
        }
    }
}