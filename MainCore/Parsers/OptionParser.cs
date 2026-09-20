namespace MainCore.Parsers
{
    public static class OptionParser
    {
        public static ILocator IsContextualHelpEnable(IPage page)
        {
            var node = page.Locator("#contextualHelp");
            return node;
        }

        public static ILocator GetOptionButton(IPage page)
        {
            var button = page.Locator("#outOfGame a.options");
            return button;
        }

        public static ILocator GetHideContextualHelpOption(IPage page)
        {
            var node = page.Locator("#hideContextualHelp");
            return node;
        }

        public static ILocator GetSubmitButton(IPage page)
        {
            var button = page.Locator(".submitButtonContainer button");
            return button;
        }
    }
}