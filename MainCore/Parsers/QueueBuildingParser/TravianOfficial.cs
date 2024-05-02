using HtmlAgilityPack;
using MainCore.DTO;

namespace MainCore.Parsers.QueueBuildingParser
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : IQueueBuildingParser
    {
        public IEnumerable<QueueBuildingDto> Get(HtmlDocument doc)
        {
            var nodes = GetNodes(doc);

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var type = GetBuildingType(node);
                var level = GetLevel(node);
                var duration = GetDuration(node);
                yield return new()
                {
                    Position = i,
                    Type = type,
                    Level = level,
                    CompleteTime = DateTime.Now.Add(duration),
                    Location = -1,
                };
            }

            for (int i = nodes.Count; i < 4; i++) // we will save 3 slot for each village, Roman can build 3 building in one time
            {
                yield return new()
                {
                    Position = i,
                    Type = "Site",
                    Level = -1,
                    CompleteTime = DateTime.MaxValue,
                    Location = -1,
                };
            }
        }

        private static List<HtmlNode> GetNodes(HtmlDocument doc)
        {
            var finishButton = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("finishNow"));
            if (finishButton is null) return new();
            return finishButton.ParentNode.Descendants("li").ToList();
        }

        private static string GetBuildingType(HtmlNode node)
        {
            var nodeName = node.Descendants("div").FirstOrDefault(x => x.HasClass("name"));
            if (nodeName is null) return "";

            return new string(nodeName.ChildNodes[0].InnerText.Where(c => char.IsLetter(c) || char.IsDigit(c)).ToArray());
        }

        private static int GetLevel(HtmlNode node)
        {
            var nodeLevel = node.Descendants("span").FirstOrDefault(x => x.HasClass("lvl"));
            if (nodeLevel is null) return 0;

            return int.Parse(new string(nodeLevel.InnerText.Where(c => char.IsDigit(c)).ToArray()));
        }

        private static TimeSpan GetDuration(HtmlNode node)
        {
            var nodeTimer = node.Descendants().FirstOrDefault(x => x.HasClass("timer"));
            if (nodeTimer is null) return TimeSpan.Zero;
            var strSec = new string(nodeTimer.GetAttributeValue("value", "0").Where(c => char.IsNumber(c)).ToArray());
            if (string.IsNullOrEmpty(strSec)) return TimeSpan.Zero;
            int sec = int.Parse(strSec);
            return TimeSpan.FromSeconds(sec);
        }
    }
}