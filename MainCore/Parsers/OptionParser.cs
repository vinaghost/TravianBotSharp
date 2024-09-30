﻿namespace MainCore.Parsers
{
    public static class OptionParser
    {
        public static bool IsContextualHelpEnable(IChromeBrowser chromeBrowser)
        {
            var html = chromeBrowser.Html;
            var node = html.GetElementbyId("contextualHelp");
            return node is not null;
        }

        public static HtmlNode GetOptionButton(HtmlDocument doc)
        {
            var outOfGame = doc.GetElementbyId("outOfGame");
            if (outOfGame is null) return null;
            var a = outOfGame.Descendants("a").FirstOrDefault(x => x.HasClass("options"));
            return a;
        }

        public static HtmlNode GetHideContextualHelpOption(HtmlDocument doc)
        {
            var node = doc.GetElementbyId("hideContextualHelp");
            return node;
        }

        public static HtmlNode GetSubmitButton(HtmlDocument doc)
        {
            var div = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("submitButtonContainer"));
            if (div is null) return null;
            var button = div.Descendants("button").FirstOrDefault();
            return button;
        }
    }
}