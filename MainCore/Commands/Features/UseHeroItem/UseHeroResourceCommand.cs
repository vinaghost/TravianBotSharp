using MainCore.Common.Errors.Storage;

namespace MainCore.Commands.Features.UseHeroItem
{
    public class UseHeroResourceCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public UseHeroResourceCommand(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public async Task<Result> Execute(AccountId accountId, IChromeBrowser chromeBrowser, long[] requiredResource, CancellationToken cancellationToken)
        {
            var currentUrl = chromeBrowser.CurrentUrl;
            Result result;
            result = await new ToHeroInventoryCommand().Execute(accountId, chromeBrowser, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            for (var i = 0; i < 4; i++)
            {
                requiredResource[i] = RoundUpTo100(requiredResource[i]);
            }

            result = IsEnoughResource(accountId, requiredResource);
            if (result.IsFailed)
            {
                if (!result.HasError<Retry>())
                {
                    var chromeResult = await chromeBrowser.Navigate(currentUrl, cancellationToken);
                    if (chromeResult.IsFailed) return chromeResult.WithError(TraceMessage.Error(TraceMessage.Line()));
                }
                return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            var items = new List<(HeroItemEnums, long)>()
            {
                (HeroItemEnums.Wood, requiredResource[0]),
                (HeroItemEnums.Clay, requiredResource[1]),
                (HeroItemEnums.Iron, requiredResource[2]),
                (HeroItemEnums.Crop, requiredResource[3]),
            };

            foreach (var item in items)
            {
                result = await UseResource(accountId, chromeBrowser, item.Item1, item.Item2, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            result = await chromeBrowser.Navigate(currentUrl, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> UseResource(AccountId accountId, IChromeBrowser chromeBrowser, HeroItemEnums item, long amount, CancellationToken cancellationToken)
        {
            if (amount == 0) return Result.Ok();
            Result result;
            result = await ClickItem(chromeBrowser, item, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var delayClickCommand = new DelayClickCommand();

            await delayClickCommand.Execute(accountId);

            result = await EnterAmount(chromeBrowser, amount);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await delayClickCommand.Execute(accountId);

            result = await Confirm(chromeBrowser, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await delayClickCommand.Execute(accountId);

            return Result.Ok();
        }

        private async Task<Result> ClickItem(IChromeBrowser chromeBrowser, HeroItemEnums item, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var node = GetItemSlot(html, item);
            if (node is null) return Retry.NotFound($"{item}", "item");

            bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return !HeroInventoryLoading(doc);
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath), loadingCompleted, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> EnterAmount(IChromeBrowser chromeBrowser, long amount)
        {
            var html = chromeBrowser.Html;
            var node = GetAmountBox(html);
            if (node is null) return Retry.TextboxNotFound("amount resource input");
            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(node.XPath), amount.ToString());
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> Confirm(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var node = GetConfirmButton(html);
            if (node is null) return Retry.ButtonNotFound("confirm use resource");

            bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return !HeroInventoryLoading(doc);
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath), loadingCompleted, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static long RoundUpTo100(long res)
        {
            if (res == 0) return 0;
            var remainder = res % 100;
            return res + (100 - remainder);
        }

        private Result IsEnoughResource(AccountId accountId, long[] requiredResource)
        {
            using var context = _contextFactory.CreateDbContext();
            var types = new List<HeroItemEnums>() {
                HeroItemEnums.Wood,
                HeroItemEnums.Clay,
                HeroItemEnums.Iron,
                HeroItemEnums.Crop,
            };

            var items = context.HeroItems
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => types.Contains(x.Type))
                .OrderBy(x => x.Type)
                .ToList();

            var result = Result.Ok();
            var wood = items.Find(x => x.Type == HeroItemEnums.Wood);
            var woodAmount = wood?.Amount ?? 0;
            if (woodAmount < requiredResource[0]) result.WithError(Resource.Error("wood", woodAmount, requiredResource[0]));
            var clay = items.Find(x => x.Type == HeroItemEnums.Clay);
            var clayAmount = clay?.Amount ?? 0;
            if (clayAmount < requiredResource[1]) result.WithError(Resource.Error("clay", clayAmount, requiredResource[1]));
            var iron = items.Find(x => x.Type == HeroItemEnums.Iron);
            var ironAmount = iron?.Amount ?? 0;
            if (ironAmount < requiredResource[2]) result.WithError(Resource.Error("iron", ironAmount, requiredResource[2]));
            var crop = items.Find(x => x.Type == HeroItemEnums.Crop);
            var cropAmount = crop?.Amount ?? 0;
            if (cropAmount < requiredResource[3]) result.WithError(Resource.Error("crop", cropAmount, requiredResource[3]));
            return result;
        }

        private static bool HeroInventoryLoading(HtmlDocument doc)
        {
            var inventoryPageWrapper = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("inventoryPageWrapper"));
            if (inventoryPageWrapper is null) return false;
            return inventoryPageWrapper.HasClass("loading");
        }

        private static HtmlNode GetItemSlot(HtmlDocument doc, HeroItemEnums type)
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

                if (itemValue.ParseInt() == (int)type) return itemSlot;
            }
            return null;
        }

        private static HtmlNode GetAmountBox(HtmlDocument doc)
        {
            var form = doc.GetElementbyId("consumableHeroItem");
            return form.Descendants("input").FirstOrDefault();
        }

        private static HtmlNode GetConfirmButton(HtmlDocument doc)
        {
            var dialog = doc.GetElementbyId("dialogContent");
            var buttonWrapper = dialog.Descendants("div").FirstOrDefault(x => x.HasClass("buttonsWrapper"));
            if (buttonWrapper is null) return null;
            var buttonTransfer = buttonWrapper.Descendants("button");
            if (buttonTransfer.Count() < 2) return null;
            return buttonTransfer.ElementAt(1);
        }
    }
}