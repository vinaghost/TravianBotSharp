using HtmlAgilityPack;
using MainCore.Common.Enums;
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
    }
}