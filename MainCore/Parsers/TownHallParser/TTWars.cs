using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.TownHallParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : ITownHallParser
    {
        public HtmlNode GetHoldButton(HtmlDocument doc, bool big)
        {
            throw new NotImplementedException();
        }
    }
}