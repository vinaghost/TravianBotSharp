using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Common.Extensions;
using MainCore.Common.Models;
using MainCore.Infrasturecture.AutoRegisterDi;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace MainCore.Parsers.RallypointParser
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : IRallypointParser
    {
        public List<IncomingAttack> GetIncomingAttacks(HtmlDocument doc)
        {
            var now = GetServerTime(doc);
            var local = DateTime.Now;
            var tables = GetTables(doc);

            var attacks = new List<IncomingAttack>();
            foreach (var table in tables)
            {
                var name = GetNameAttacker(table);
                var x = GetX(table);
                var y = GetY(table);
                var arrivalTime = GetArrivalTime(table);
                var arrival = now.Add(arrivalTime);
                var localArrival = local.Add(arrivalTime);

                var attack = attacks.LastOrDefault(x => x.VillageName == name);

                if (attack is null)
                {
                    attacks.Add(new()
                    {
                        VillageName = name,
                        X = x,
                        Y = y,
                        ArrivalTime = arrival,
                        LocalTime = localArrival,
                        WaveCount = 1,
                        Type = table.HasClass("inAttack") ? TroopMovementEnums.Attack : TroopMovementEnums.Raid,
                        IsNew = true,
                    });
                }
                else
                {
                    var diff = attack.ArrivalTime > arrival ? attack.ArrivalTime - arrival : arrival - attack.ArrivalTime;
                    if (diff > TimeSpan.FromSeconds(2))
                    {
                        attacks.Add(new()
                        {
                            VillageName = name,
                            X = x,
                            Y = y,
                            ArrivalTime = arrival,
                            LocalTime = localArrival,
                            WaveCount = 1,
                            Type = table.HasClass("inAttack") ? TroopMovementEnums.Attack : TroopMovementEnums.Raid,
                            IsNew = true,
                        });
                    }
                    else
                    {
                        attack.WaveCount++;
                        var diffSecond = (int)diff.TotalSeconds;
                        attack.ArrivalTime = attack.ArrivalTime < arrival ? attack.ArrivalTime : arrival;
                        attack.LocalTime = attack.LocalTime < localArrival ? attack.LocalTime : localArrival;
                        attack.DelaySecond = attack.DelaySecond < diffSecond ? diffSecond : attack.DelaySecond;
                    }
                }
            }
            return attacks;
        }

        private static List<HtmlNode> GetTables(HtmlDocument doc)
        {
            var tables = doc.DocumentNode
                .Descendants("table")
                .Where(x => x.HasClass("troop_details") && (x.HasClass("inAttack") || x.HasClass("inRaid")))
                .ToList();
            return tables;
        }

        private static string GetNameAttacker(HtmlNode node)
        {
            var td = node
                .Descendants("td")
                .FirstOrDefault(x => x.HasClass("role"));
            if (td is null) return "?";
            return td.InnerText.Trim();
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

        private static DateTime GetServerTime(HtmlDocument doc)
        {
            var serverTime = doc.GetElementbyId("servertime");
            var timer = serverTime
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("timer"));

            var dur = timer.InnerText.ToDuration();
            return DateTime.Today.Add(dur);
        }

        public IEnumerable<HtmlNode> GetTroopInput(HtmlDocument doc)
        {
            var inputs = doc.DocumentNode
                .Descendants("input")
                .Where(x => x.GetAttributeValue("disabled", "_") != "");

            for (var i = 1; i <= 11; i++)
            {
                var node = inputs.FirstOrDefault(x => x.GetAttributeValue("name", "") == $"troop[t{i}]");
                if (node is null) continue;
                yield return node;
            }
        }

        public int GetTroopAmount(HtmlNode input)
        {
            var a = input.NextSibling.NextSibling;
            if (a.Name != "a") return 0;

            return a.InnerText.ToInt();
        }

        public HtmlNode GetConfirmButton(HtmlDocument doc)
        {
            var button = doc.DocumentNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("rallyPointConfirm"));
            return button;
        }

        public HtmlNode GetCancelButton(HtmlDocument doc)
        {
            var div = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("abort"));
            if (div is null) return null;

            return div
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("icon"));
        }

        public HtmlNode GetXInput(HtmlDocument doc)
        {
            return doc.GetElementbyId("xCoordInput");
        }

        public HtmlNode GetYInput(HtmlDocument doc)
        {
            return doc.GetElementbyId("yCoordInput");
        }

        public HtmlNode GetRaidInput(HtmlDocument doc)
        {
            return doc.DocumentNode
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == "eventType" && x.GetAttributeValue("value", 0) == 4);
        }

        public HtmlNode GetSendButton(HtmlDocument doc)
        {
            return doc.GetElementbyId("ok");
        }
    }
}