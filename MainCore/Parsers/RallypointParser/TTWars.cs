using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Common.Models;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.RallypointParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : IRallypointParser
    {
        public HtmlNode GetRaidInput(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public HtmlNode GetCancelButton(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public HtmlNode GetConfirmButton(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public List<IncomingAttack> GetIncomingAttacks(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public int GetTroopAmount(HtmlNode input)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<HtmlNode> GetTroopInput(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public HtmlNode GetXInput(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public HtmlNode GetYInput(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public HtmlNode GetSendButton(HtmlDocument doc)
        {
            throw new NotImplementedException();
        }
    }
}