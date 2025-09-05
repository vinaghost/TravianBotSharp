namespace MainCore.Parsers
{
    public static class InfoParser
    {
        public static int GetGold(HtmlDocument doc)
        {
            var goldNode = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("ajaxReplaceableGoldAmount"));
            if (goldNode is null) return -1;
            return goldNode.InnerText.ParseInt();
        }

        public static int GetSilver(HtmlDocument doc)
        {
            var silverNode = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("ajaxReplaceableSilverAmount"));
            if (silverNode is null) return -1;
            return silverNode.InnerText.ParseInt();
        }

        public static bool HasPlusAccount(HtmlDocument doc)
        {
            var boxLink = doc.GetElementbyId("sidebarBoxLinklist");
            if (boxLink is null) return false;
            var editButton = boxLink.Descendants("a").FirstOrDefault(x => x.HasClass("edit") && x.HasClass("round"));
            if (editButton is null) return false;

            if (editButton.HasClass("green")) return true;
            if (editButton.HasClass("gold")) return false;
            return false;
        }
    }
}
