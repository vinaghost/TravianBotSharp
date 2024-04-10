using HtmlAgilityPack;
using MainCore.Common.Extensions;
using MainCore.Common.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace MainCore.Parsers.RallypointParser
{
    public class RallypointParser
    {
        public List<IncomingAttack> GetIncomingAttacks(HtmlDocument doc)
        {
            var now = GetServerTime(doc);
            var tables = GetTables(doc);

            var attacks = new List<IncomingAttack>();
            foreach (var table in tables)
            {
                var name = GetNameAttacker(table);
                var x = GetX(table);
                var y = GetY(table);
                var arrival = now.Add(GetArrivalTime(table));

                var attack = attacks.FirstOrDefault(x => x.PlayerName == name);

                if (attack is null)
                {
                    attacks.Add(new()
                    {
                        PlayerName = name,
                        X = x,
                        Y = y,
                        ArrivalTime = arrival,
                        WaveCount = 1,
                    });
                }
                else
                {
                    var diff = attack.ArrivalTime > arrival ? attack.ArrivalTime - arrival : arrival - attack.ArrivalTime;
                    if (diff > TimeSpan.FromSeconds(2))
                    {
                        attacks.Add(new()
                        {
                            PlayerName = name,
                            X = x,
                            Y = y,
                            ArrivalTime = arrival,
                            WaveCount = 1,
                        });
                    }
                    else
                    {
                        attack.WaveCount++;
                        var diffSecond = (int)diff.TotalSeconds;
                        attack.ArrivalTime = attack.ArrivalTime < arrival ? attack.ArrivalTime : arrival;
                        attack.DelaySecond = attack.DelaySecond < diffSecond ? diffSecond : attack.DelaySecond;
                    }
                }
            }
            return attacks;
        }

        private List<HtmlNode> GetTables(HtmlDocument doc)
        {
            var tables = doc.DocumentNode
                .Descendants("table")
                .Where(x => x.HasClass("troop_details") && (x.HasClass("inAttack") || x.HasClass("inRaid")))
                .ToList();
            return tables;
        }

        private string GetNameAttacker(HtmlNode node)
        {
            var td = node
                .Descendants("td")
                .FirstOrDefault(x => x.HasClass("role"));
            if (td is null) return "?";
            return td.InnerText;
        }

        private static int GetX(HtmlNode node)
        {
            var xNode = node.Descendants("span").FirstOrDefault(x => x.HasClass("coordinateX"));
            if (xNode is null) return 0;
            return xNode.InnerText.ToInt();
        }

        private static int GetY(HtmlNode node)
        {
            var yNode = node.Descendants("span").FirstOrDefault(x => x.HasClass("coordinateY"));
            if (yNode is null) return 0;
            return yNode.InnerText.ToInt();
        }

        private static TimeSpan GetArrivalTime(HtmlNode node)
        {
            var timer = node
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("timer"));

            return timer.InnerText.ToDuration();
        }

        private DateTime GetServerTime(HtmlDocument doc)
        {
            var serverTime = doc.GetElementbyId("servertime");
            var timer = serverTime
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("timer"));

            var dur = timer.InnerText.ToDuration();
            return DateTime.Today.Add(dur);
        }
    }
}