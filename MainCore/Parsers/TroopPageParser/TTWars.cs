using HtmlAgilityPack;

namespace MainCore.Parsers.TroopPageParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : ITroopPageParser
    {
        public HtmlNode GetInputBox(HtmlDocument doc, TroopEnums troop)
        {
            var node = GetNode(doc, troop);
            var cta = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("cta"));
            if (cta is null) return null;
            var input = cta.Descendants("input")
                .FirstOrDefault(x => x.HasClass("text"));
            return input;
        }

        public int GetMaxAmount(HtmlDocument doc, TroopEnums troop)
        {
            var node = GetNode(doc, troop);
            var cta = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("cta"));
            if (cta is null) return 0;
            var a = cta.Descendants("a")
                .FirstOrDefault();
            return a.InnerText.ToInt();
        }

        public TimeSpan GetQueueTrainTime(HtmlDocument doc)
        {
            var table = doc.DocumentNode.Descendants("table")
                .FirstOrDefault(x => x.HasClass("under_progress"));
            if (table is null) return TimeSpan.FromSeconds(0);
            var td = table.Descendants("td")
                .FirstOrDefault(x => x.HasClass("dur"));
            var timer = td.Descendants("span")
                .FirstOrDefault(x => x.HasClass("timer"));
            var value = timer.GetAttributeValue("value", 0);
            return TimeSpan.FromSeconds(value);
        }

        public HtmlNode GetTrainButton(HtmlDocument doc)
        {
            return doc.GetElementbyId("s1");
        }

        public long[] GetTrainCost(HtmlDocument doc, TroopEnums troop)
        {
            var node = GetNode(doc, troop);
            var resource = node.Descendants("div")
                .Where(x => x.HasClass("inlineIcon"))
                .Where(x => x.HasClass("resource"))
                .SkipLast(1) // free crop
                .ToList();
            var resources = new long[4];
            for (var i = 0; i < 4; i++)
            {
                var span = resource[i].Descendants("span")
                    .Where(x => x.HasClass("value"))
                    .FirstOrDefault();
                var text = span.InnerText;
                resources[i] = text.ToLong();
            }
            return resources;
        }

        public TimeSpan GetTrainTime(HtmlDocument doc, TroopEnums troop)
        {
            var node = GetNode(doc, troop);
            var durationDiv = node.Descendants("div")
                .Where(x => x.HasClass("duration"))
                .FirstOrDefault();
            var durationSpan = durationDiv.Descendants("span")
                .Where(x => x.HasClass("value"))
                .FirstOrDefault();
            return durationSpan.InnerText.ToDuration();
        }

        private static HtmlNode GetNode(HtmlDocument doc, TroopEnums troop)
        {
            var nodes = doc.DocumentNode.Descendants("div")
               .Where(x => x.HasClass("troop"))
               .Where(x => !x.HasClass("empty"))
               .AsEnumerable();

            foreach (var node in nodes)
            {
                var img = node.Descendants("img")
                .FirstOrDefault(x => x.HasClass("unit"));
                var classes = img.GetClasses();
                var type = classes
                    .Where(x => x.StartsWith("u"))
                    .FirstOrDefault(x => !x.Equals("unit"));
                if (type.ToInt() == (int)troop) return node;
            }
            return null;
        }
    }
}