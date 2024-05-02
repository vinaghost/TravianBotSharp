using HtmlAgilityPack;

namespace MainCore.Parsers.LoginPageParser
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : ILoginPageParser
    {
        public HtmlNode GetUsernameNode(HtmlDocument doc)
        {
            var node = doc.DocumentNode
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "").Equals("name"));
            return node;
        }

        public HtmlNode GetPasswordNode(HtmlDocument doc)
        {
            var node = doc.DocumentNode
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "").Equals("password"));
            return node;
        }

        public HtmlNode GetLoginButton(HtmlDocument doc)
        {
            var trNode = doc.DocumentNode
                .Descendants("tr")
                .FirstOrDefault(x => x.HasClass("loginButtonRow"));
            if (trNode is null) return null;
            var node = trNode
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("green"));
            return node;
        }
    }
}