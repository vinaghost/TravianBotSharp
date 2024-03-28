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

        public int GetProgressingSettlerAmount(HtmlDocument doc, TroopEnums troop)
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetProgressingSettlerCompleteTime(HtmlDocument doc, TroopEnums troop)
        {
            throw new NotImplementedException();
        }

        public DateTime GetSettleArrivalTime(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public HtmlNode GetSettleButton(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public int GetSettlerAmount(HtmlDocument doc, TroopEnums troop)
        {
            throw new NotImplementedException();
        }

        public bool IsSettlerEnough(HtmlDocument doc, TroopEnums troop)
        {
            throw new NotImplementedException();
        }
    }
}