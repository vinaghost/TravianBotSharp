using HtmlAgilityPack;

namespace MainCore.Parsers.LoginPageParser
{
    
    public class TTWars : ILoginPageParser
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
            var node = doc.GetElementbyId("s1");
            return node;
        }
    }
}