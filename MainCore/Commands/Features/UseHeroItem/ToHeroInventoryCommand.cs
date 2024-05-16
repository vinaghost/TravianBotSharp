namespace MainCore.Commands.Features.UseHeroItem
{
    public class ToHeroInventoryCommand
    {
        private readonly IMediator _mediator;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ToHeroInventoryCommand(IMediator mediator = null, IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _mediator = mediator ?? Locator.Current.GetService<IMediator>();
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public async Task<Result> Execute(AccountId accountId, IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            Result result;
            result = await ToHeroInventory(chromeBrowser, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await UpdateHeroInventory(accountId, chromeBrowser, cancellationToken);
            return Result.Ok();
        }

        private async Task<Result> ToHeroInventory(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var avatar = GetHeroAvatar(html);
            if (avatar is null) return Retry.ButtonNotFound("avatar hero");

            bool tabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryTabActive(doc);
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(avatar.XPath), "hero", tabActived, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task UpdateHeroInventory(AccountId accountId, IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var dtos = GetItems(html);
            Update(accountId, dtos.ToList());
            await _mediator.Publish(new HeroItemUpdated(accountId), cancellationToken);
        }

        private static bool InventoryTabActive(HtmlDocument doc)
        {
            var heroDiv = doc.GetElementbyId("heroV2");
            if (heroDiv is null) return false;
            var aNode = heroDiv.Descendants("a")
                .FirstOrDefault(x => x.GetAttributeValue("data-tab", 0) == 1);
            if (aNode is null) return false;
            return aNode.HasClass("active");
        }

        private static HtmlNode GetHeroAvatar(HtmlDocument doc)
        {
            return doc.GetElementbyId("heroImageButton");
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