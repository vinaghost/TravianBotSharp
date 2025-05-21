namespace MainCore.Parsers
{
    public static class LoginParser
    {
        public static HtmlNode? GetLoginButton(HtmlDocument doc)
        {
            var loginScene = doc.GetElementbyId("loginScene");
            if (loginScene is null) return null;

            var loginButton = loginScene
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("green"));

            return loginButton;
        }

        public static HtmlNode? GetUsernameInput(HtmlDocument doc)
        {
            var usernameInput = doc.DocumentNode
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "").Equals("name"));
            return usernameInput;
        }

        public static HtmlNode? GetPasswordInput(HtmlDocument doc)
        {
            var passwordInput = doc.DocumentNode
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "").Equals("password"));
            return passwordInput;
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