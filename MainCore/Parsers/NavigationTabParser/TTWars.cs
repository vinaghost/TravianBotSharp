using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;

namespace MainCore.Parsers.NavigationTabParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : INavigationTabParser
    {
        private static HtmlNode GetNavigationBar(HtmlDocument doc)
        {
            var navigationBar = doc.DocumentNode
             .Descendants("div")
             .FirstOrDefault(x => x.HasClass("contentNavi") && x.HasClass("subNavi"));
            return navigationBar;
        }

        private static IEnumerable<HtmlNode> GetTabs(HtmlDocument doc)
        {
            var navigationBar = GetNavigationBar(doc);
            if (navigationBar is null) return Enumerable.Empty<HtmlNode>();
            var tabs = navigationBar
                .Descendants("a")
                .Where(x => x.HasClass("tabItem"));
            return tabs;
        }

        public int CountTab(HtmlDocument doc)
        {
            var count = GetTabs(doc)
                .Count();
            return count;
        }

        public HtmlNode GetTab(HtmlDocument doc, int index)
        {
            var tab = GetTabs(doc)
                .ElementAt(index);
            return tab;
        }

        public bool IsTabActive(HtmlNode node)
        {
            return node.HasClass("active");
        }
    }
}