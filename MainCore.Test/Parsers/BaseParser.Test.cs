using HtmlAgilityPack;

namespace MainCore.Test.Parsers
{
    public abstract class BaseParser
    {
        protected readonly HtmlDocument _html;

        protected BaseParser()
        {
            _html = new HtmlDocument();
        }
    }
}