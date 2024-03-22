using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.SettleParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : ISettleParser
    {
        public IEnumerable<ExpansionSlotDto> Get(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }
    }
}