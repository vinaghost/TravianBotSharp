namespace MainCore.Parsers
{
    public static class OptionParser
    {
        public static bool IsContextualHelpEnable(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("contextualHelp");
            return node is not null;
        }

        public static HtmlNode? GetOptionButton(HtmlDocument doc)
        {
            var outOfGame = doc.GetElementbyId("outOfGame");
            if (outOfGame is null) return null;
            var optionButton = outOfGame
                .Descendants("a")
                .FirstOrDefault(x => x.HasClass("options"));
            return optionButton;
        }

        public static HtmlNode GetHideContextualHelpOption(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("hideContextualHelp");
            return node;
        }

        public static HtmlNode? GetSubmitButton(HtmlDocument doc)
        {
            var submitButtonContainer = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("submitButtonContainer"));
            if (submitButtonContainer is null) return null;

            var submitButton = submitButtonContainer
                .Descendants("button")
                .FirstOrDefault();
            return submitButton;
        }
    }
}
