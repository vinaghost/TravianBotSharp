using HtmlAgilityPack;
using MainCore.DTO;

namespace MainCore.Parsers
{
    public interface IQueueBuildingParser
    {
        IEnumerable<QueueBuildingDto> Get(HtmlDocument doc);
    }
}