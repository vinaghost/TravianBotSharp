using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.HeroParser
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : IHeroParser
    {
        public TimeSpan GetAdventureDuration(HtmlDocument doc)
        {
            var heroAdventure = doc.GetElementbyId("heroAdventure");
            var timer = heroAdventure
                .Descendants("span")
                .Where(x => x.HasClass("timer"))
                .FirstOrDefault();
            if (timer is null) return TimeSpan.Zero;

            var seconds = timer.GetAttributeValue("value", 0);
            return TimeSpan.FromSeconds(seconds);
        }

        public HtmlNode GetContinueButton(HtmlDocument doc)
        {
            var button = doc.DocumentNode
                .Descendants("button")
                .Where(x => x.HasClass("continue"))
                .FirstOrDefault();
            return button;
        }

        public HtmlNode GetHeroAdventure(HtmlDocument doc)
        {
            var adventure = doc.DocumentNode
                .Descendants("a")
                .Where(x => x.HasClass("adventure") && x.HasClass("round"))
                .FirstOrDefault();
            return adventure;
        }

        public bool CanStartAdventure(HtmlDocument doc)
        {
            var status = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.HasClass("heroStatus"))
                .FirstOrDefault();
            if (status is null) return false;

            var heroHome = status.Descendants("i")
                .Where(x => x.HasClass("heroHome"))
                .Any();
            if (!heroHome) return false;

            var adventure = GetHeroAdventure(doc);
            if (adventure is null) return false;

            var adventureAvailabe = adventure.Descendants("div")
                .Where(x => x.HasClass("content"))
                .Any();
            return adventureAvailabe;
        }

        public bool IsDead(HtmlDocument doc)
        {
            var status = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.HasClass("heroStatus"))
                .FirstOrDefault();
            if (status is null) return false;

            var heroDead = status.Descendants("i")
                .Where(x => x.HasClass("heroDead"))
                .Any();
            if (!heroDead) return false;
            return true;
        }

        public HtmlNode GetAdventure(HtmlDocument doc)
        {
            var adventures = doc.GetElementbyId("heroAdventure");
            if (adventures is null) return null;

            var tbody = adventures.Descendants("tbody").FirstOrDefault();
            if (tbody is null) return null;

            var tr = tbody.Descendants("tr").FirstOrDefault();
            if (tr is null) return null;
            var button = tr.Descendants("button").FirstOrDefault();
            return button;
        }

        public string GetAdventureInfo(HtmlNode node)
        {
            var difficult = GetAdventureDifficult(node);
            var coordinates = GetAdventureCoordinates(node);

            return $"{difficult} - {coordinates}";
        }

        private static string GetAdventureDifficult(HtmlNode node)
        {
            var tdList = node.Descendants("td").ToArray();
            if (tdList.Length < 3) return "unknown";
            var iconDifficulty = tdList[3].FirstChild;
            return iconDifficulty.GetAttributeValue("alt", "unknown");
        }

        private static string GetAdventureCoordinates(HtmlNode node)
        {
            var tdList = node.Descendants("td").ToArray();
            if (tdList.Length < 2) return "[~|~]";
            return tdList[1].InnerText;
        }

        public bool InventoryTabActive(HtmlDocument doc)
        {
            var heroDiv = doc.GetElementbyId("heroV2");
            if (heroDiv is null) return false;
            var aNode = heroDiv.Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("data-tab", 0) == 1);
            if (aNode is null) return false;
            return aNode.HasClass("active");
        }

        public bool AttributeTabActive(HtmlDocument doc)
        {
            var heroDiv = doc.GetElementbyId("heroV2");
            if (heroDiv is null) return false;
            var aNode = heroDiv.Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("data-tab", 0) == 2);
            if (aNode is null) return false;
            return aNode.HasClass("active");
        }

        public HtmlNode GetHeroAttributeNode(HtmlDocument doc)
        {
            var heroDiv = doc.GetElementbyId("heroV2");
            if (heroDiv is null) return null;
            var aNode = heroDiv.Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("data-tab", 0) == 2);
            return aNode;
        }

        public bool HeroInventoryLoading(HtmlDocument doc)
        {
            var inventoryPageWrapper = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("inventoryPageWrapper"));
            return inventoryPageWrapper.HasClass("loading");
        }

        public HtmlNode GetHeroAvatar(HtmlDocument doc)
        {
            return doc.GetElementbyId("heroImageButton");
        }

        public HtmlNode GetItemSlot(HtmlDocument doc, HeroItemEnums type)
        {
            var heroItemsDiv = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("heroItems"));
            if (heroItemsDiv is null) return null;
            var heroItemDivs = heroItemsDiv.Descendants("div").Where(x => x.HasClass("heroItem") && !x.HasClass("empty"));
            if (!heroItemDivs.Any()) return null;

            foreach (var itemSlot in heroItemDivs)
            {
                if (itemSlot.ChildNodes.Count < 2) continue;
                var itemNode = itemSlot.ChildNodes[1];
                var classes = itemNode.GetClasses();
                if (classes.Count() != 2) continue;

                var itemValue = classes.ElementAt(1);

                var itemValueStr = new string(itemValue.Where(c => char.IsDigit(c)).ToArray());
                if (string.IsNullOrEmpty(itemValueStr)) continue;

                if (int.Parse(itemValueStr) == (int)type) return itemSlot;
            }
            return null;
        }

        public HtmlNode GetAmountBox(HtmlDocument doc)
        {
            var form = doc.GetElementbyId("consumableHeroItem");
            return form.Descendants("input").FirstOrDefault();
        }

        public HtmlNode GetConfirmButton(HtmlDocument doc)
        {
            var dialog = doc.GetElementbyId("dialogContent");
            var buttonWrapper = dialog.Descendants("div").FirstOrDefault(x => x.HasClass("buttonsWrapper"));
            var buttonTransfer = buttonWrapper.Descendants("button");
            if (buttonTransfer.Count() < 2) return null;
            return buttonTransfer.ElementAt(1);
        }

        public IEnumerable<HeroItemDto> GetItems(HtmlDocument doc)
        {
            var heroItemsDiv = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("heroItems"));
            if (heroItemsDiv is null) yield break;
            var heroItemDivs = heroItemsDiv.Descendants("div").Where(x => x.HasClass("heroItem") && !x.HasClass("empty"));
            if (!heroItemDivs.Any()) yield break;

            foreach (var itemSlot in heroItemDivs)
            {
                if (itemSlot.ChildNodes.Count < 2) continue;
                var itemNode = itemSlot.ChildNodes[1];
                var classes = itemNode.GetClasses();
                if (classes.Count() != 2) continue;

                var itemValue = classes.ElementAt(1);
                if (itemValue is null) continue;

                var itemValueStr = new string(itemValue.Where(c => char.IsDigit(c)).ToArray());
                if (string.IsNullOrEmpty(itemValueStr)) continue;

                if (!itemSlot.GetAttributeValue("data-tier", "").Contains("consumable"))
                {
                    yield return new HeroItemDto()
                    {
                        Type = (HeroItemEnums)int.Parse(itemValueStr),
                        Amount = 1,
                    };
                    continue;
                }

                if (itemSlot.ChildNodes.Count < 3)
                {
                    yield return new HeroItemDto()
                    {
                        Type = (HeroItemEnums)int.Parse(itemValueStr),
                        Amount = 1,
                    };
                    continue;
                }
                var amountNode = itemSlot.ChildNodes[2];

                var amountValueStr = new string(amountNode.InnerText.Where(c => char.IsDigit(c)).ToArray());
                if (string.IsNullOrEmpty(amountValueStr))
                {
                    yield return new HeroItemDto()
                    {
                        Type = (HeroItemEnums)int.Parse(itemValueStr),
                        Amount = 1,
                    };
                    continue;
                }
                yield return new HeroItemDto()
                {
                    Type = (HeroItemEnums)int.Parse(itemValueStr),
                    Amount = int.Parse(amountValueStr),
                };
                continue;
            }
        }

        public HtmlNode GetFightingStrengthInputBox(HtmlDocument doc)
        {
            var inputBox = doc.DocumentNode.Descendants("input").FirstOrDefault(x => x.GetAttributeValue("name", "") == "fightingStrength");
            return inputBox;
        }

        public HtmlNode GetOffBonusInputBox(HtmlDocument doc)
        {
            var inputBox = doc.DocumentNode.Descendants("input").FirstOrDefault(x => x.GetAttributeValue("name", "") == "offBonus");
            return inputBox;
        }

        public HtmlNode GetDefBonusInputBox(HtmlDocument doc)
        {
            var inputBox = doc.DocumentNode.Descendants("input").FirstOrDefault(x => x.GetAttributeValue("name", "") == "defBonus");
            return inputBox;
        }

        public HtmlNode GetResourceProductionInputBox(HtmlDocument doc)
        {
            var inputBox = doc.DocumentNode.Descendants("input").FirstOrDefault(x => x.GetAttributeValue("name", "") == "resourceProduction");
            return inputBox;
        }

        public HtmlNode GetSaveButton(HtmlDocument doc)
        {
            var button = doc.GetElementbyId("savePoints");
            return button;
        }

        public bool IsLevelUp(HtmlDocument doc)
        {
            var topBarHero = doc.GetElementbyId("topBarHero");
            if (topBarHero is null) return false;
            var levelUp = topBarHero.Descendants("i").FirstOrDefault(x => x.HasClass("levelUp"));
            if (levelUp is null) return false;
            return levelUp.HasClass("show");
        }

        public long[] GetRevivedResource(HtmlDocument doc)
        {
            var reviveWrapper = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("reviveWrapper"));
            if (reviveWrapper is null) return Array.Empty<long>();
            var reviveWithResources = reviveWrapper
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("reviveWithResources") && x.HasClass("charges"));
            if (reviveWithResources is null) return Array.Empty<long>();

            var resourceDivs = reviveWithResources
                .Descendants("div")
                .Where(x => x.HasClass("resource"))
                .Take(4);
            if (!resourceDivs.Any()) return Array.Empty<long>();

            var resources = new long[5];
            for (var i = 0; i < 4; i++)
            {
                var resourceDiv = resourceDivs.ElementAt(i);
                var resourceValue = resourceDiv
                    .Descendants("span")
                    .FirstOrDefault();
                if (resourceValue is null)
                {
                    resources[i] = 0;
                    continue;
                }
                var resourceValueStr = new string(resourceValue.InnerText.Where(c => char.IsDigit(c)).ToArray());
                if (string.IsNullOrEmpty(resourceValueStr)) continue;
                resources[i] = long.Parse(resourceValueStr);
            }

            resources[4] = 0; // free crop
            return resources;
        }

        public HtmlNode GetReviveButton(HtmlDocument doc)
        {
            var reviveWrapper = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("reviveWrapper"));
            if (reviveWrapper is null) return null;
            var reviveWithResourcesButton = reviveWrapper
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("reviveWithResources") && x.HasClass("green"));
            return reviveWithResourcesButton;
        }
    }
}