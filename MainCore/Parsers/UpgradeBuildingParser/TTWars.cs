using HtmlAgilityPack;

namespace MainCore.Parsers.UpgradeBuildingParser
{
    
    public class TTWars : IUpgradeBuildingParser
    {
        public long[] GetRequiredResource(HtmlDocument doc, bool isEmptySite, BuildingEnums building = BuildingEnums.Site)
        {
            HtmlNode node;
            if (isEmptySite)
            {
                node = doc.GetElementbyId($"contract_building{(int)building}");
            }
            else
            {
                node = doc.GetElementbyId("contract");
            }

            if (node is null) return Enumerable.Repeat(-1L, 5).ToArray();
            var resourceWrapper = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("resourceWrapper"));
            if (resourceWrapper is null) return Enumerable.Repeat(-1L, 5).ToArray();

            var resources = resourceWrapper.Descendants("div")
                .Where(x => x.HasClass("resource"))
                .ToList();

            if (resources.Count != 5) return Enumerable.Repeat(-1L, 5).ToArray();

            var resourceBuilding = new long[5];
            for (var i = 0; i < 5; i++)
            {
                resourceBuilding[i] = resources[i].InnerText.ParseLong();
            }

            return resourceBuilding;
        }

        public TimeSpan GetTimeWhenEnoughResource(HtmlDocument doc, bool isEmptySite, BuildingEnums building = BuildingEnums.Site)
        {
            HtmlNode node;
            if (isEmptySite)
            {
                node = doc.GetElementbyId($"contract_building{(int)building}");
            }
            else
            {
                node = doc.GetElementbyId("contract");
            }
            if (node is null) return TimeSpan.Zero;

            var errorMessage = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("errorMessage"));
            if (errorMessage is null) return TimeSpan.Zero;
            var timer = errorMessage.Descendants("span")
                .FirstOrDefault(x => x.HasClass("timer"));
            if (timer is null) return TimeSpan.Zero;
            var time = timer.GetAttributeValue("value", 0);
            return TimeSpan.FromSeconds(time);
        }

        public HtmlNode GetSpecialUpgradeButton(HtmlDocument doc)
        {
            var node = doc.DocumentNode.Descendants("div")
               .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (node is null) return null;

            var button = node.Descendants("button")
                .FirstOrDefault(x => x.HasClass("upgradeToMaxLevel"));

            return button;
        }

        public HtmlNode GetUpgradeButton(HtmlDocument doc)
        {
            var node = doc.DocumentNode.Descendants("div")
               .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (node is null) return null;

            var button = node.Descendants("button")
                .FirstOrDefault(x => x.HasClass("build"));

            return button;
        }

        public HtmlNode GetConstructButton(HtmlDocument doc, BuildingEnums building = BuildingEnums.Site)
        {
            var node = doc.GetElementbyId($"contract_building{(int)building}");
            if (node is null) return null;

            var button = node.Descendants("button")
                .FirstOrDefault(x => x.HasClass("new"));

            return button;
        }
    }
}