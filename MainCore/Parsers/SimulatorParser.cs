using System.Text.RegularExpressions;

namespace MainCore.Parsers
{
    public static class SimulatorParser
    {
        public static bool IsSimulator(HtmlDocument doc)
        {
            var form = doc.GetElementbyId("combatSimulatorForm");
            return form is not null;
        }

        public static IEnumerable<HtmlNode> GetAttackerInput(HtmlDocument doc)
        {
            var div = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("attack") && x.HasClass("troops"));
            if (div is null) return [];
            var tr = div
                .Descendants("tr")
                .FirstOrDefault(x => x.HasClass("troopUnits"));
            if (tr is null) return [];
            return tr.Descendants("input")
                .Where(x => x.GetAttributeValue("name", "").StartsWith("unit"));
        }

        public static HtmlNode? GetSimulateButton(HtmlDocument doc)
        {
            var button = doc.GetElementbyId("simulate");
            return button;
        }

        public static HtmlNode? GetSendTroopsButton(HtmlDocument doc)
        {
            var button = doc.GetElementbyId("sendTroops");
            return button;
        }

        public static int GetHeroHealth(HtmlDocument doc)
        {
            var i = doc.DocumentNode
                .Descendants("i")
                .LastOrDefault(x => x.HasClass("hero_health"));
            if (i is null) return 0;

            var span = i.NextSibling;
            if (span is null) return 0;

            var text = span.InnerText.Trim();

            return ExtractNumberAfterTo(text);
        }

        public static int ExtractNumberAfterTo(string input)
        {
            // This regex looks for "to" followed by whitespace and then captures the number at the end of the string
            var match = Regex.Match(input, @"\bto\s+(\d+)$", RegexOptions.Compiled);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int result))
            {
                return result;
            }
            return 0;
        }

        public static int GetRewardResource(HtmlDocument doc)
        {
            var div = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("additionalRewards"));
            if (div is null) return 0;

            var span = div
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("value"));
            if (span is null) return 0;

            var text = span.InnerText.Trim();
            return text.ParseInt() * 4;
        }
    }
}