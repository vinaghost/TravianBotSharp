namespace MainCore.Commands.Abstract
{
    public abstract class LoginPageCommand
    {
        protected static HtmlNode GetLoginButton(HtmlDocument doc)
        {
            var loginScene = doc.GetElementbyId("loginScene");
            if (loginScene is null) return null;

            var node = loginScene
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("green"));
            return node;
        }
    }
}