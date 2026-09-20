using Microsoft.Playwright;

namespace MainCore.Parsers
{
    public static class LoginParser
    {
        public static ILocator GetLoginButton(IPage page)
        {
            var button = page.Locator("#loginScene button.green");
            return button;
        }

        public static ILocator GetUsernameInput(IPage page)
        {
            var usernameInput = page.Locator("input[name='name']");
            return usernameInput;
        }

        public static ILocator GetPasswordInput(IPage page)
        {
            var passwordInput = page.Locator("input[name='password']");
            return passwordInput;
        }

        public static ILocator GetServerTime(IPage page)
        {
            var serverTime = page.Locator("#servertime");
            return serverTime;
        }
    }
}