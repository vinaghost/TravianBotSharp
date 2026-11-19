namespace MainCore.Features.Login
{
    public static class Parser
    {
        public static async Task<bool> IsLoginPage(this IPage page)
        {
            var loginScene = page.GetLoginButton();
            var count = await loginScene.CountAsync();
            return count > 0;
        }

        public static async Task<bool> IsIngamePage(this IPage page)
        {
            var serverTime = page.GetServerTime();
            var count = await serverTime.CountAsync();
            return count > 0;
        }

        public static ILocator GetLoginButton(this IPage page)
        {
            var button = page.Locator("#loginScene button.green");
            return button;
        }

        public static ILocator GetUsernameInput(this IPage page)
        {
            var usernameInput = page.Locator("input[name='name']");
            return usernameInput;
        }

        public static ILocator GetPasswordInput(this IPage page)
        {
            var passwordInput = page.Locator("input[name='password']");
            return passwordInput;
        }

        public static ILocator GetCookieConsentAcceptButton(this IPage page)
        {
            var acceptButton = page.Locator("div#cmpwrapper .cmpboxbtn.cmpboxbtnyes.cmptxt_btn_yes");
            return acceptButton;
        }

        public static ILocator GetConstructionPlansContinueButton(this IPage page)
        {
            var continueButton = page.Locator("#constructionPlansSpawnedAnnouncement #constructionPlansButton");
            return continueButton;
        }

        public static ILocator GetContextualHelp(this IPage page)
        {
            var node = page.Locator("#contextualHelp");
            return node;
        }

        public static ILocator GetOptionButton(this IPage page)
        {
            var button = page.Locator("#outOfGame a.options");
            return button;
        }

        public static ILocator GetHideContextualHelpOption(this IPage page)
        {
            var node = page.Locator("#hideContextualHelp");
            return node;
        }

        public static ILocator GetSubmitButton(this IPage page)
        {
            var button = page.Locator(".submitButtonContainer button");
            return button;
        }
    }
}