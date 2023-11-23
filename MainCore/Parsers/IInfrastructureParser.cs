using HtmlAgilityPack;
using MainCore.DTO;

namespace MainCore.Parsers
{
    public interface IInfrastructureParser
    {
        IEnumerable<BuildingDto> Get(HtmlDocument doc);
    }
}