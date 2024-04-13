using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Common.Models;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.RallypointParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : IRallypointParser
    {
        public List<IncomingAttack> GetIncomingAttacks(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }
    }
}