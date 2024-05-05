using MainCore.Commands.Abstract;

namespace MainCore.Commands.Queries
{
    public class IsLoginPage : LoginPageCommand
    {
        public bool Execute(IChromeBrowser chromeBrowser)
        {
            var html = chromeBrowser.Html;
            var loginButton = GetLoginButton(html);
            return loginButton is not null;
        }
    }
}