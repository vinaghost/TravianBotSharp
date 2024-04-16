using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.AllianceParser
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : IAllianceParser
    {
        public HtmlNode GetAllianceButton(HtmlDocument doc)
        {
            return doc.DocumentNode
                .Descendants("a")
                .Where(x => x.HasClass("alliance"))
                .FirstOrDefault();
        }

        private static readonly Dictionary<AllianceBonusEnums, string> _allianceBonusInput = new()
        {
            {AllianceBonusEnums.Recruitment, "bonusTroopProductionSpeed" },
            {AllianceBonusEnums.Philosophy, "bonusCPProduction" },
            {AllianceBonusEnums.Metallurgy, "bonusSmithyPower" },
            {AllianceBonusEnums.Commerce, "bonusMerchantCapacity" },
        };

        public HtmlNode GetBonusSelector(HtmlDocument doc, AllianceBonusEnums bonus)
        {
            return doc.DocumentNode
                .Descendants("label")
                .FirstOrDefault(x => x.GetAttributeValue("for", "") == _allianceBonusInput[bonus]);
        }

        public IEnumerable<HtmlNode> GetBonusInputs(HtmlDocument doc)
        {
            for (int i = 1; i < 5; i++)
            {
                var node = doc.GetElementbyId($"donate{i}");
                yield return node;
            }
        }

        public HtmlNode GetContributeButton(HtmlDocument doc)
        {
            return doc.GetElementbyId("donate_green");
        }
    }
}