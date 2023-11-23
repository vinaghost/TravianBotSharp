using HtmlAgilityPack;
using MainCore.DTO;

namespace MainCore.Parsers
{
    public interface IAccountInfoParser
    {
        AccountInfoDto Get(HtmlDocument doc);
    }
}