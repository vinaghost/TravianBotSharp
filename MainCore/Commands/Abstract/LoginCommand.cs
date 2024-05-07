namespace MainCore.Commands.Abstract
{
    public abstract class LoginPageCommand
    {
        protected static HtmlNode GetLoginButton(HtmlDocument doc)
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