using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.AllianceParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : IAllianceParser
    {
        public HtmlNode GetAllianceButton(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<HtmlNode> GetBonusInputs(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public HtmlNode GetBonusSelector(HtmlDocument doc, AllianceBonusEnums bonus)
        {
            throw new NotImplementedException();
        }

        public HtmlNode GetContributeButton(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }
    }
}