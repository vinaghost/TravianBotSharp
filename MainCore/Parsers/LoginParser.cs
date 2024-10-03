namespace MainCore.Parsers
{
    public static class LoginParser
    {
        public static HtmlNode GetLoginButton(HtmlDocument doc)
        {
            var loginScene = doc.GetElementbyId("loginScene");
            if (loginScene is null) return null;

            var node = loginScene
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("green"));
            return node;
        }

        public static HtmlNode GetUsernameInput(HtmlDocument doc)
        {
            var node = doc.DocumentNode
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "").Equals("name"));
            return node;
        }

        public static HtmlNode GetPasswordInput(HtmlDocument doc)
        {
            var node = doc.DocumentNode
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "").Equals("password"));
            return node;
        }

        public static bool IsIngamePage(HtmlDocument doc)
        {
            var serverTime = doc.GetElementbyId("servertime");
            return serverTime is not null;
        }

        public static bool IsLoginPage(HtmlDocument doc)
        {
            var loginButton = GetLoginButton(doc);
            return loginButton is not null;
        }
    }
}