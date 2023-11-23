using HtmlAgilityPack;
using MainCore.DTO;

namespace MainCore.Parsers
{
    public interface IStockBarParser
    {
        StorageDto Get(HtmlDocument doc);
    }
}