using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.AcademyParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : IAcademyParser
    {
        public HtmlNode GetResearchButton(HtmlDocument doc, TroopEnums troop)
        {
            throw new NotImplementedException();
        }
    }
}