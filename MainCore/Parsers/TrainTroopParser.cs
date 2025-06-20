namespace MainCore.Parsers
{
    public static class TrainTroopParser
    {
        public static HtmlNode? GetInputBox(HtmlDocument doc, TroopEnums troop)
        {
            var node = GetNode(doc, troop);
            if (node is null) return null;
            var cta = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("cta"));
            if (cta is null) return null;

            var input = cta.Descendants("input")
                .FirstOrDefault(x => x.HasClass("text"));
            return input;
        }

        public static int GetMaxAmount(HtmlDocument doc, TroopEnums troop)
        {
            var node = GetNode(doc, troop);
            if (node is null) return 0;
            var cta = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("cta"));
            if (cta is null) return 0;
            var a = cta.Descendants("a")
                .FirstOrDefault();
            if (a is null) return 0;

            return a.InnerText.ParseInt();
        }

        public static HtmlNode GetTrainButton(HtmlDocument doc)
        {
            return doc.GetElementbyId("s1");
        }

        public static TimeSpan GetQueueTime(HtmlDocument doc)
        {
            var timers = doc.DocumentNode
                .Descendants("span")
                .Where(x => x.HasClass("timer"))
                .Where(x => x.GetAttributeValue("counting", "") == "down")
                .Select(x => x.GetAttributeValue("value", 0));

            if (!timers.Any()) return TimeSpan.Zero;

            var seconds = timers.Max();
            return TimeSpan.FromSeconds(seconds);
        }

        private static HtmlNode? GetNode(HtmlDocument doc, TroopEnums troop)
        {
            var nodes = doc.DocumentNode.Descendants("div")
               .Where(x => x.HasClass("troop"))
               .Where(x => !x.HasClass("empty"))
               .AsEnumerable();

            foreach (var node in nodes)
            {
                var img = node.Descendants("img")
                .FirstOrDefault(x => x.HasClass("unit"));
                if (img is null) continue;
                var classes = img.GetClasses();
                var type = classes
                    .Where(x => x.StartsWith('u'))
                    .FirstOrDefault(x => !x.Equals("unit"));
                if (type is null) continue;
                if (type.ParseInt() == (int)troop) return node;
            }
            return null;
        }
    }
}