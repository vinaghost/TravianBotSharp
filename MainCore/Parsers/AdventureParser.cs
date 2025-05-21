namespace MainCore.Parsers
{
    public static class AdventureParser
    {
        public static TimeSpan GetAdventureDuration(HtmlDocument doc)
        {
            var heroAdventure = doc.GetElementbyId("heroAdventure");
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

        public static HtmlNode? GetHeroAdventureButton(HtmlDocument doc)
        {
            var adventureButton = doc.DocumentNode
                .Descendants("a")
                .FirstOrDefault(x => x.HasClass("adventure") && x.HasClass("round"));
            return adventureButton;
        }

        public static bool CanStartAdventure(HtmlDocument doc)
        {
            var heroStatus = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("heroStatus"));
            if (heroStatus is null) return false;
            var heroHome = heroStatus.Descendants("i")
                .Any(x => x.HasClass("heroHome"));
            if (!heroHome) return false;

            var adventureButton = GetHeroAdventureButton(doc);
            if (adventureButton is null) return false;

            var adventureAvailabe = adventureButton.Descendants("div")
                .Any(x => x.HasClass("content"));
            return adventureAvailabe;
        }

        public static HtmlNode? GetAdventureButton(HtmlDocument doc)
        {
            var adventureTable = doc.GetElementbyId("heroAdventure");
            if (adventureTable is null) return null;

            var adventureTableBody = adventureTable
                .Descendants("tbody")
                .FirstOrDefault();
            if (adventureTableBody is null) return null;

            var adventureTableBodyRow = adventureTableBody
                .Descendants("tr")
                .FirstOrDefault();
            if (adventureTableBodyRow is null) return null;

            var startAdventureButton = adventureTableBodyRow
                .Descendants("button")
                .FirstOrDefault();
            return startAdventureButton;
        }

        public static HtmlNode? GetContinueButton(HtmlDocument doc)
        {
            var continueButton = doc.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("continue"));
            return continueButton;
        }

        public static string GetAdventureInfo(HtmlNode node)
        {
            // adventureTableBodyRow/td/buton
            var trNode = node.ParentNode.ParentNode;
            var difficult = GetAdventureDifficult(trNode);
            var coordinates = GetAdventureCoordinates(trNode);

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