namespace MainCore.Commands.Abstract
{
    public abstract class NavigationBarCommand
    {
        protected static HtmlNode GetButton(HtmlDocument doc, int key)
        {
            var navigationBar = doc.GetElementbyId("navigation");
            if (navigationBar is null) return null;
            var buttonNode = navigationBar.Descendants("a").FirstOrDefault(x => x.GetAttributeValue("accesskey", 0) == key);
            return buttonNode;
        }
    }
}