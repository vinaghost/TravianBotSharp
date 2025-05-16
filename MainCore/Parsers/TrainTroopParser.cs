using Humanizer;

namespace MainCore.Parsers
{
    public static class TrainTroopParser
    {
        public static HtmlNode GetInputBox(HtmlDocument doc, TroopEnums troop)
        {
            var node = GetNode(doc, troop);
            var cta = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("cta"));
            BrokenParserException.ThrowIfNull(cta);
            var input = cta.Descendants("input")
                .FirstOrDefault(x => x.HasClass("text"));
            BrokenParserException.ThrowIfNull(input);
            return input;
        }

        public static int GetMaxAmount(HtmlDocument doc, TroopEnums troop)
        {
            var node = GetNode(doc, troop);
            var cta = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("cta"));
            BrokenParserException.ThrowIfNull(cta);
            var a = cta.Descendants("a")
                .FirstOrDefault();
            BrokenParserException.ThrowIfNull(a);
            return a.InnerText.ParseInt();
        }

        public static HtmlNode GetTrainButton(HtmlDocument doc)
        {
            return doc.GetElementbyId("s1");
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
                if (img is null) continue;
                var classes = img.GetClasses();
                var type = classes
                    .Where(x => x.StartsWith('u'))
                    .FirstOrDefault(x => !x.Equals("unit"));
                if (type is null) continue;
                if (type.ParseInt() == (int)troop) return node;
            }

            throw BrokenParserException.NotFound($"{troop.Humanize()} node");
        }
    }
}