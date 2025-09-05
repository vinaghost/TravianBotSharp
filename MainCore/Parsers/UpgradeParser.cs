namespace MainCore.Parsers
{
    public static class UpgradeParser
    {
        private static HtmlNode GetContractNode(HtmlDocument doc, BuildingEnums building)
        {
            var node = doc.GetElementbyId($"contract_building{(int)building}"); // building
            node ??= doc.GetElementbyId("contract"); // site
            return node;
        }

        public static List<HtmlNode> GetRequiredResource(HtmlDocument doc, BuildingEnums building)
        {
            var node = GetContractNode(doc, building);

            if (node is null) return [];
            var resourceWrapper = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("resourceWrapper"));
            if (resourceWrapper is null) return [];

            var resources = resourceWrapper.Descendants("div")
                .Where(x => x.HasClass("resource"))
                .ToList();

            if (resources.Count != 5) return [];
            return resources;
        }

        public static TimeSpan GetTimeWhenEnoughResource(HtmlDocument doc, BuildingEnums building)
        {
            var node = GetContractNode(doc, building);

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

        public static HtmlNode? GetConstructButton(HtmlDocument doc, BuildingEnums building)
        {
            if (building.IsResourceField()) return GetUpgradeButton(doc);

            // Önce spesifik bina kontrat alanýný arýyorum
            var contract_building = doc.GetElementbyId($"contract_building{(int)building}");
            if (contract_building is not null)
            {
                var button = contract_building
                    .Descendants("button")
                    .FirstOrDefault(x => x.HasClass("new"));
                if (button is not null) return button;
            }

            // Spesifik bulamadýysam, genel contract alanýnda spesifik binayý arýyorum
            var contract = doc.GetElementbyId("contract");
            if (contract is not null)
            {
                // Bina ismini ve ID'sini kullanarak doðru butonu buluyorum
                var buildingId = (int)building;
                var buildingName = building.ToString().ToLower();
                
                // Önce onclick attribute'unda bina ID'si olan buton arýyorum
                var specificButton = contract
                    .Descendants("button")
                    .FirstOrDefault(x => x.HasClass("new") && 
                        (x.GetAttributeValue("onclick", "").Contains($"gid={buildingId}") ||
                         x.GetAttributeValue("onclick", "").Contains($"gid%3D{buildingId}")));
                
                if (specificButton is not null) return specificButton;

                // Alternatif olarak form action'ýnda bina ID'si olan buton arýyorum
                var formButton = contract
                    .Descendants("button")
                    .Where(x => x.HasClass("new"))
                    .FirstOrDefault(x => 
                    {
                        var form = x.Ancestors("form").FirstOrDefault();
                        return form?.GetAttributeValue("action", "").Contains($"gid={buildingId}") == true;
                    });

                if (formButton is not null) return formButton;

                // Son çare: genel "new" butonu (eski davranýþ)
                var generalButton = contract
                    .Descendants("button")
                    .FirstOrDefault(x => x.HasClass("new"));
                if (generalButton is not null) return generalButton;
            }

            return null;
        }

        public static HtmlNode? GetSpecialUpgradeButton(HtmlDocument doc)
        {
            var upgradeButtonsContainer = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (upgradeButtonsContainer is null) return null;

            var button = upgradeButtonsContainer
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("videoFeatureButton") && x.HasClass("green"));
            return button;
        }

        public static HtmlNode? GetUpgradeButton(HtmlDocument doc)
        {
            var upgradeButtonsContainer = doc.DocumentNode.Descendants("div")
               .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (upgradeButtonsContainer is null) return null;

            var button = upgradeButtonsContainer.Descendants("button")
                .FirstOrDefault(x => x.HasClass("build"));
            return button;
        }
    }
}
