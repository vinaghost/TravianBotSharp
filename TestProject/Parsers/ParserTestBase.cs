using HtmlAgilityPack;

namespace TestProject.Parsers
{
    public class ParserTestBase<TParser> where TParser : new()
    {
        protected static string[] parts;

        protected static (TParser, HtmlDocument) Setup(string filename)
        {
            var parser = new TParser();
            var html = new HtmlDocument();
            var path = Helper.GetPath(parts, filename);
            html.Load(path);
            return (parser, html);
        }
    }
}