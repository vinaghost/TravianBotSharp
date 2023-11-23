using HtmlAgilityPack;
using MainCore.DTO;

namespace MainCore.Parsers
{
    public interface IFieldParser
    {
        IEnumerable<BuildingDto> Get(HtmlDocument doc);
    }
}