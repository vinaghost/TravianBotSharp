namespace MainCore.Parsers
{
    public static class OptionParser
    {
        public static bool IsContextualHelpEnable(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("contextualHelp");
            return node is not null;
        }

        public static HtmlNode GetOptionButton(HtmlDocument doc)
        {
            var outOfGame = doc.GetElementbyId("outOfGame");
            BrokenParserException.ThrowIfNull(outOfGame);
            var optionButton = outOfGame
                .Descendants("a")
                .FirstOrDefault(x => x.HasClass("options"));
            BrokenParserException.ThrowIfNull(optionButton);
            return optionButton;
        }

        public static HtmlNode GetHideContextualHelpOption(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("hideContextualHelp");
            return node;
        }

        public static HtmlNode GetSubmitButton(HtmlDocument doc)
        {
            var submitButtonContainer = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("submitButtonContainer"));
            BrokenParserException.ThrowIfNull(submitButtonContainer);
            var submitButton = submitButtonContainer
                .Descendants("button")
                .FirstOrDefault();
            BrokenParserException.ThrowIfNull(submitButton);
            return submitButton;
        }
    }
}