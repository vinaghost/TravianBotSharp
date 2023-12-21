using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers.HeroParser
{
    [RegisterAsTransient(ServerEnums.TTWars)]
    public class TTWars : IHeroParser
    {
        public bool InventoryTabActive(HtmlDocument doc)
        {
            return true;
        }

        public bool HeroInventoryLoading(HtmlDocument doc)
        {
            return true;
        }

        public HtmlNode GetHeroAvatar(HtmlDocument doc)
        {
            return doc.GetElementbyId("heroImageButton");
        }

        public HtmlNode GetItemSlot(HtmlDocument doc, HeroItemEnums type)
        {
            var inventory = doc.GetElementbyId("itemsToSale");
            foreach (var itemSlot in inventory.ChildNodes)
            {
                var item = itemSlot.ChildNodes.FirstOrDefault(x => x.Id.StartsWith("item_"));
                if (item is null) continue;

                var itemClass = item.GetClasses().FirstOrDefault(x => x.Contains("_item_"));
                var itemValue = itemClass.Split('_').LastOrDefault();
                if (itemValue is null) continue;

                var itemValueStr = new string(itemValue.Where(c => char.IsDigit(c)).ToArray());
                if (string.IsNullOrEmpty(itemValueStr)) continue;

                if (int.Parse(itemValueStr) == (int)type) return item;
            }
            return null;
        }

        public HtmlNode GetAmountBox(HtmlDocument doc)
        {
            return doc.GetElementbyId("amount");
        }

        public HtmlNode GetConfirmButton(HtmlDocument doc)
        {
            return doc.DocumentNode.Descendants("button").FirstOrDefault(x => x.HasClass("ok"));
        }

        public IEnumerable<HeroItemDto> GetItems(HtmlDocument doc)
        {
            var inventory = doc.GetElementbyId("itemsToSale");
            if (inventory is null) yield break;

            foreach (var itemSlot in inventory.ChildNodes)
            {
                var item = itemSlot.ChildNodes.FirstOrDefault(x => x.Id.StartsWith("item_"));
                if (item is null) continue;

                var itemClass = item.GetClasses().FirstOrDefault(x => x.Contains("_item_"));
                var itemValue = itemClass.Split('_').LastOrDefault();
                if (itemValue is null) continue;

                var itemValueStr = new string(itemValue.Where(c => char.IsDigit(c)).ToArray());
                if (string.IsNullOrEmpty(itemValueStr)) continue;

                var amountValue = item.ChildNodes.FirstOrDefault(x => x.HasClass("amount"));
                if (amountValue is null)
                {
                    yield return new HeroItemDto()
                    {
                        Type = (HeroItemEnums)int.Parse(itemValueStr),
                        Amount = 1,
                    };
                    continue;
                }

                var amountValueStr = new string(amountValue.InnerText.Where(c => char.IsDigit(c)).ToArray());
                if (string.IsNullOrEmpty(itemValueStr))
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
            }
        }

        public HtmlNode GetHeroAdventure(HtmlDocument doc)
        {
            return doc.DocumentNode.Descendants().FirstOrDefault(x => x.HasClass("adventure"));
        }

        public bool CanStartAdventure(HtmlDocument doc)
        {
            var heroHome = doc.DocumentNode
                .Descendants("svg")
                .Any(x => x.HasClass("heroHome"));

            if (!heroHome) return false;

            var adventure = GetHeroAdventure(doc);
            if (adventure is null) return false;

            var adventureAvailabe = adventure.Descendants("div")
                .Where(x => x.HasClass("content"))
                .Any();
            return adventureAvailabe;
        }

        public HtmlNode GetAdventure(HtmlDocument doc)
        {
            var adventures = doc.GetElementbyId("adventureListForm");
            if (adventures is null) return null;
            var list = adventures.Descendants("tr").ToList();
            list.RemoveAt(0);
            if (list.Count == 0) return null;
            var tr = list[0];
            var button = tr.Descendants("a").FirstOrDefault(x => x.HasClass("gotoAdventure"));
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
            var img = node.Descendants("img").FirstOrDefault(x => x.HasClass("adventureDifficulty1"));
            if (img is null) return "unknown";
            return img.GetAttributeValue("alt", "unknown");
        }

        private static string GetAdventureCoordinates(HtmlNode node)
        {
            var coordsNode = node.Descendants("td").FirstOrDefault(x => x.HasClass("coords"));
            if (coordsNode is null) return "[~|~]";
            return coordsNode.InnerText;
        }

        public HtmlNode GetContinueButton(HtmlDocument doc)
        {
            var div = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("adventureSendButton"));

            if (div is null) return null;

            var button = div
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("green"));
            return button;
        }

        public TimeSpan GetAdventureDuration(HtmlDocument doc)
        {
            var heroAdventure = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("heroStatusMessage"));

            if (heroAdventure is null) return TimeSpan.Zero;

            var timer = heroAdventure
                .Descendants("span")
                .Where(x => x.HasClass("timer"))
                .FirstOrDefault();
            if (timer is null) return TimeSpan.Zero;

            var seconds = timer.GetAttributeValue("value", 0);
            return TimeSpan.FromSeconds(seconds);
        }
    }
}