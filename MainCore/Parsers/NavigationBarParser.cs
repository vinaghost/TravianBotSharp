namespace MainCore.Parsers
{
    public static class NavigationBarParser
    {
        public static HtmlNode? GetDorfButton(HtmlDocument doc, int dorf)
        {
            return dorf switch
            {
                1 => GetResourceButton(doc),
                2 => GetBuildingButton(doc),
                _ => null,
            };
        }

        private static HtmlNode? GetButton(HtmlDocument doc, int key)
        {
            var navigationBar = doc.GetElementbyId("navigation");
            if (navigationBar is null) return null;

            var button = navigationBar
                .Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("accesskey", 0) == key);
            return button;
        }

        private static HtmlNode? GetResourceButton(HtmlDocument doc) => GetButton(doc, 1);

        private static HtmlNode? GetBuildingButton(HtmlDocument doc) => GetButton(doc, 2);
    }
}