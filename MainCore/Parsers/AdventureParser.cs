namespace MainCore.Parsers
{
    public static class AdventureParser
    {
        public static TimeSpan GetAdventureDuration(IChromeBrowser chromeBrowser)
        {
            var html = chromeBrowser.Html;
            var heroAdventure = html.GetElementbyId("heroAdventure");
            var timer = heroAdventure
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("timer"));
            if (timer is null) return TimeSpan.Zero;

            var seconds = timer.GetAttributeValue("value", 0);
            return TimeSpan.FromSeconds(seconds);
        }

        public static bool IsAdventurePage(HtmlDocument doc)
        {
            var table = doc.DocumentNode
                .Descendants("table")
                .Any(x => x.HasClass("adventureList"));
            return table;
        }

        public static HtmlNode GetHeroAdventure(HtmlDocument doc)
        {
            var adventure = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.HasClass("adventure") && x.HasClass("round"));
            return adventure;
        }

        public static bool CanStartAdventure(HtmlDocument doc)
        {
            var status = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("heroStatus"));
            if (status is null) return false;

            var heroHome = status.Descendants("i")
                .Any(x => x.HasClass("heroHome"));
            if (!heroHome) return false;

            var adventure = GetHeroAdventure(doc);
            if (adventure is null) return false;

            var adventureAvailabe = adventure.Descendants("div")
                .Any(x => x.HasClass("content"));
            return adventureAvailabe;
        }

        public static HtmlNode GetAdventure(HtmlDocument doc)
        {
            var adventures = doc.GetElementbyId("heroAdventure");
            if (adventures is null) return null;

            var tbody = adventures.Descendants("tbody").FirstOrDefault();
            if (tbody is null) return null;

            var tr = tbody.Descendants("tr").FirstOrDefault();
            if (tr is null) return null;
            var button = tr.Descendants("button").FirstOrDefault();
            return button;
        }

        public static HtmlNode GetContinueButton(HtmlDocument doc)
        {
            var button = doc.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("continue"));
            return button;
        }

        public static string GetAdventureInfo(HtmlNode node)
        {
            var difficult = GetAdventureDifficult(node);
            var coordinates = GetAdventureCoordinates(node);

            return $"{difficult} - {coordinates}";
        }

        private static string GetAdventureDifficult(HtmlNode node)
        {
            var tdList = node.Descendants("td").ToArray();
            if (tdList.Length < 3) return "unknown";
            var iconDifficulty = tdList[3].FirstChild;
            return iconDifficulty.GetAttributeValue("alt", "unknown");
        }

        private static string GetAdventureCoordinates(HtmlNode node)
        {
            var tdList = node.Descendants("td").ToArray();
            if (tdList.Length < 2) return "[~|~]";
            return tdList[1].InnerText;
        }
    }
}