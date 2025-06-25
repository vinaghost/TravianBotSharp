namespace MainCore.Parsers
{
    public static class RallypointParser
    {
        public static HtmlNode? GetConfirmButton(HtmlDocument doc)
        {
            var button = doc.GetElementbyId("confirmSendTroops");
            return button;
        }

        public static HtmlNode? GetSendButton(HtmlDocument doc)
        {
            var button = doc.GetElementbyId("ok");
            return button;
        }

        public static TimeSpan GetHeroTime(HtmlDocument doc)
        {
            var tables = doc.DocumentNode
                .Descendants("table")
                .Where(x => x.HasClass("troop_details") && x.HasClass("outRaid"));

            foreach (var table in tables)
            {
                var td = table.Descendants("td")
                    .FirstOrDefault(x => x.HasClass("unit") && x.HasClass("last"));

                if (td is null) continue;
                if (!td.InnerText.Contains("1")) continue;

                var nodeTimer = table.Descendants("span")
                    .FirstOrDefault(x => x.HasClass("timer"));
                if (nodeTimer is null) return TimeSpan.Zero;
                int sec = nodeTimer.GetAttributeValue("value", 0);
                return TimeSpan.FromSeconds(sec);
            }

            return TimeSpan.Zero;
        }
    }
}