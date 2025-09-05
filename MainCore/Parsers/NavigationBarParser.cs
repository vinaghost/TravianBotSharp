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
            if (navigationBar is null) 
            {
                // Debug: Navigation bar bulunamadı
                Console.WriteLine($"[DEBUG] NavigationBarParser: Navigation bar not found for key={key}");
                return null;
            }

            var buttons = navigationBar.Descendants("a").ToList();
            Console.WriteLine($"[DEBUG] NavigationBarParser: Found {buttons.Count} buttons in navigation bar");
            
            foreach (var btn in buttons)
            {
                var accesskey = btn.GetAttributeValue("accesskey", 0);
                var href = btn.GetAttributeValue("href", "");
                Console.WriteLine($"[DEBUG] NavigationBarParser: Button accesskey={accesskey}, href={href}");
            }

            var button = buttons.FirstOrDefault(x => x.GetAttributeValue("accesskey", 0) == key);
            if (button is null)
            {
                Console.WriteLine($"[DEBUG] NavigationBarParser: Button with accesskey={key} not found");
            }
            else
            {
                Console.WriteLine($"[DEBUG] NavigationBarParser: Button with accesskey={key} found");
            }
            
            return button;
        }

        private static HtmlNode? GetResourceButton(HtmlDocument doc) => GetButton(doc, 1);

        private static HtmlNode? GetBuildingButton(HtmlDocument doc) => GetButton(doc, 2);
    }
}
