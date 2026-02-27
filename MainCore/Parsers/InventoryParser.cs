namespace MainCore.Parsers
{
    public static class InventoryParser
    {
        private static readonly IReadOnlyDictionary<HeroItemEnums, string> ResourceInputNames = new Dictionary<HeroItemEnums, string>
        {
            { HeroItemEnums.Wood, "lumber" },
            { HeroItemEnums.Clay, "clay" },
            { HeroItemEnums.Iron, "iron" },
            { HeroItemEnums.Crop, "crop" },
        };

        public static bool IsInventoryPage(HtmlDocument doc)
        {
            var heroDiv = doc.GetElementbyId("heroV2");
            if (heroDiv is null) return false;
            var aNode = heroDiv.Descendants("a")
                .FirstOrDefault(x => x.HasClass("tabItem"));
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

        public static HtmlNode? GetAmountBox(HtmlDocument doc, HeroItemEnums item)
        {
            var dialog = GetResourceTransferDialog(doc);
            if (dialog is null) return null;
            if (!ResourceInputNames.TryGetValue(item, out var inputName)) return null;

            return dialog
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "").Equals(inputName, StringComparison.OrdinalIgnoreCase));
        }

        public static HtmlNode? GetConfirmButton(HtmlDocument doc, HeroItemEnums item)
        {
            if (!ResourceInputNames.ContainsKey(item)) return null;

            var dialog = GetResourceTransferDialog(doc);
            if (dialog is null) return null;

            var actionButton = dialog
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("actionButton"));
            if (actionButton is null) return null;

            var actionButtons = actionButton.Descendants("button").ToList();
            if (actionButtons.Count == 0) return null;

            var transferButton = actionButtons.Last();
            if (IsDisabledButton(transferButton)) return null;
            return transferButton;
        }

        private static HtmlNode? GetResourceTransferDialog(HtmlDocument doc)
        {
            var dialog = GetActiveHeroDialog(doc);
            if (dialog is null) return null;

            var hasResourceRows = dialog
                .Descendants("div")
                .Any(x => x.HasClass("resourceRow"));
            if (!hasResourceRows) return null;

            return dialog;
        }

        private static HtmlNode? GetActiveHeroDialog(HtmlDocument doc)
        {
            var reactDialogWrapper = doc.GetElementbyId("reactDialogWrapper");
            if (reactDialogWrapper is null) return null;

            var overlays = reactDialogWrapper
                .Descendants("div")
                .Where(x => x.HasClass("dialogOverlay"));

            var activeOverlay = overlays.FirstOrDefault(x => x.HasClass("dialogVisible")) ?? overlays.FirstOrDefault();
            if (activeOverlay is null) return null;

            return activeOverlay
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("dialogContents")) ?? activeOverlay;
        }

        private static bool IsDisabledButton(HtmlNode button)
        {
            if (button.Attributes.Contains("disabled")) return true;
            return button.GetClasses().Any(x => x.Equals("disabled", StringComparison.OrdinalIgnoreCase));
        }
    }
}
