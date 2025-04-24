﻿using MainCore.Commands.Abstract;

namespace MainCore.Commands.Update
{
    [RegisterScoped<UpdateInventoryCommand>]
    public class UpdateInventoryCommand(IDataService dataService, IDbContextFactory<AppDbContext> contextFactory, HeroItemUpdated.Handler heroItemUpdated) : CommandBase(dataService), ICommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;
        private readonly HeroItemUpdated.Handler _heroItemUpdated = heroItemUpdated;

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var dtos = GetItems(html);
            Update(accountId, dtos.ToList());
            await _heroItemUpdated.HandleAsync(new(accountId), cancellationToken);
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

        private void Update(AccountId accountId, List<HeroItemDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
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