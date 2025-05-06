using MainCore.Constraints;

namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateInventoryCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            HeroItemUpdated.Handler heroItemUpdated,
            CancellationToken cancellationToken)
        {
            
            var html = browser.Html;

            var dtos = GetItems(html);
            Update(command.AccountId, dtos.ToList(), context);

            await heroItemUpdated.HandleAsync(new(command.AccountId), cancellationToken);
            return Result.Ok();
        }

        private static IEnumerable<HeroItemDto> GetItems(HtmlDocument doc)
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

                var item = (HeroItemEnums)itemValue.ParseInt();

                if (item == HeroItemEnums.None) continue;

                if (!itemSlot.GetAttributeValue("data-tier", "").Contains("consumable"))
                {
                    yield return new HeroItemDto()
                    {
                        Type = item,
                        Amount = 1,
                    };
                    continue;
                }

                if (itemSlot.ChildNodes.Count < 3)
                {
                    yield return new HeroItemDto()
                    {
                        Type = item,
                        Amount = 1,
                    };
                    continue;
                }
                var amountNode = itemSlot.ChildNodes[2];

                var amount = amountNode.InnerText.ParseInt();
                if (amount == 0)
                {
                    yield return new HeroItemDto()
                    {
                        Type = item,
                        Amount = 1,
                    };
                    continue;
                }
                yield return new HeroItemDto()
                {
                    Type = item,
                    Amount = amount,
                };
            }
        }

        private static void Update(AccountId accountId, List<HeroItemDto> dtos, AppDbContext context)
        {
            
            var items = context.HeroItems
                .Where(x => x.AccountId == accountId.Value)
                .ToList();

            var types = dtos.Select(x => x.Type).ToList();

            var itemDeleted = items.Where(x => !types.Contains(x.Type)).ToList();
            var itemInserted = dtos.Where(x => !items.Exists(v => v.Type == x.Type)).ToList();
            var itemUpdated = items.Where(x => types.Contains(x.Type)).ToList();

            itemDeleted.ForEach(x => context.Remove(x));
            itemInserted.ForEach(x => context.Add(x.ToEntity(accountId)));

            foreach (var item in itemUpdated)
            {
                var dto = dtos.Find(x => x.Type == item.Type);
                dto.To(item);
                context.Update(item);
            }

            context.SaveChanges();
        }
    }
}