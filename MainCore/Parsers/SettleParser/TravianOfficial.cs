using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Common.Extensions;
using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.SettleParser
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : ISettleParser
    {
        public IEnumerable<ExpansionSlotDto> Get(HtmlDocument doc)
        {
            return GetUsedExpansionSlot(doc)
                .Concat(GetFreeExpansionSlot(doc))
                .Concat(GetNextExpansionSlot(doc));
        }

        private static IEnumerable<ExpansionSlotDto> GetUsedExpansionSlot(HtmlDocument doc)
        {
            var expansionTab = doc.GetElementbyId("expansionTab");
            if (expansionTab is null) yield break;
            var expansions = expansionTab
                .Descendants("div")
                .Where(x => x.HasClass("usedExpansionSlot"));

            foreach (var expansion in expansions)
            {
                var coordNode = expansion.Descendants("td").FirstOrDefault(x => x.HasClass("coords"));

                var coordinate = coordNode?.InnerText ?? "(~|~)";
                var dateNode = expansion.Descendants("td").FirstOrDefault(x => x.HasClass("date"));
                var date = dateNode?.InnerText ?? "~/~/~";

                yield return new ExpansionSlotDto()
                {
                    Content = $"{coordinate} in {date}",
                    Status = ExpansionStatusEnum.UsedExpansionSlot,
                };
            }
        }

        private static IEnumerable<ExpansionSlotDto> GetFreeExpansionSlot(HtmlDocument doc)
        {
            var expansionTab = doc.GetElementbyId("expansionTab");
            if (expansionTab is null) yield break;
            var expansions = expansionTab
                .Descendants("div")
                .Where(x => x.HasClass("freeExpansionSlot"));

            foreach (var expansion in expansions)
            {
                yield return new ExpansionSlotDto()
                {
                    Status = ExpansionStatusEnum.FreeExpansionSlot,
                };
            }
        }

        private static IEnumerable<ExpansionSlotDto> GetNextExpansionSlot(HtmlDocument doc)
        {
            var expansionTab = doc.GetElementbyId("expansionTab");
            if (expansionTab is null) yield break;
            var expansions = expansionTab
                .Descendants("div")
                .Where(x => x.HasClass("nextExpansionSlot"));

            foreach (var expansion in expansions)
            {
                yield return new ExpansionSlotDto()
                {
                    Status = ExpansionStatusEnum.NextExpansionSlot,
                };
            }
        }

        public bool IsSettlerEnough(HtmlDocument doc, TroopEnums troop)
        {
            return GetSettlerAmount(doc, troop) + GetProgressingSettlerAmount(doc, troop) >= 3;
        }

        public int GetSettlerAmount(HtmlDocument doc, TroopEnums troop)
        {
            var troopBox = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass($"troop{(int)troop}") && x.HasClass("innerTroopWrapper"));

            if (troopBox is null) return 0;

            var title = troopBox
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("tit"));

            if (title is null) return 0;

            var amount = title
                .Descendants("span")
                .FirstOrDefault();

            if (amount is null) return 0;

            return amount.InnerText.ToInt();
        }

        public int GetProgressingSettlerAmount(HtmlDocument doc, TroopEnums troop)
        {
            var table = doc.DocumentNode
                .Descendants("table")
                .FirstOrDefault(x => x.HasClass("under_progress"));
            if (table is null) return 0;
            var tbody = table
                .Descendants("tbody")
                .FirstOrDefault();

            var troops = tbody.Descendants("img").Where(x => x.HasClass("unit")).ToList();

            var sum = 0;
            for (int i = 0; i < troops.Count; i++)
            {
                var cls = troops[i].GetClasses().FirstOrDefault(x => x != "unit");
                var progressingTroop = (TroopEnums)cls.ToInt();
                if (troop != progressingTroop) continue;
                sum += troops[i].NextSibling.InnerText.ToInt();
            }
            return sum;
        }
    }
}