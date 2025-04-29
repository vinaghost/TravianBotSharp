namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public static partial class UseHeroResourceCommand
    {
        public sealed record Command(AccountId AccountId, long[] Resource) : ICustomCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            IDbContextFactory<AppDbContext> contextFactory,
            DelayClickCommand.Handler delayClickCommand,
            CancellationToken cancellationToken)
        {
            var (accountId, resource) = command;

            for (var i = 0; i < 4; i++)
            {
                resource[i] = RoundUpTo100(resource[i]);
            }

            var result = IsEnoughResource(accountId, resource, contextFactory);
            if (result.IsFailed) return result;

            var items = new Dictionary<HeroItemEnums, long>
            {
                { HeroItemEnums.Wood, resource[0] },
                { HeroItemEnums.Clay, resource[1] },
                { HeroItemEnums.Iron, resource[2] },
                { HeroItemEnums.Crop, resource[3] },
            };

            foreach (var item in items)
            {
                result = await UseResource(item.Key, item.Value, chromeManager, delayClickCommand, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            return Result.Ok();
        }

        private static async ValueTask<Result> UseResource(
            HeroItemEnums item,
            long amount,
            IChromeManager chromeManager,
            DelayClickCommand.Handler delayClickCommand,
            CancellationToken cancellationToken)
        {
            if (amount == 0) return Result.Ok();
            Result result;
            result = await ClickItem(item, chromeManager, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await delayClickCommand.HandleAsync(cancellationToken);

            result = await EnterAmount(amount, chromeManager);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await delayClickCommand.HandleAsync(cancellationToken);

            result = await Confirm(chromeManager, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await delayClickCommand.HandleAsync(cancellationToken);

            return Result.Ok();
        }

        private static async ValueTask<Result> ClickItem(
            HeroItemEnums item,
            IChromeManager chromeManager,
            CancellationToken cancellationToken)
        {
            var html = chromeManager.Html;
            var node = InventoryParser.GetItemSlot(html, item);
            if (node is null) return Retry.NotFound($"{item}", "item");

            static bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryLoaded(doc);
            }

            Result result;
            result = await chromeManager.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeManager.Wait(driver => loadingCompleted(driver), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static async ValueTask<Result> EnterAmount(long amount, IChromeManager chromeManager)
        {
            var html = chromeManager.Html;
            var node = InventoryParser.GetAmountBox(html);
            if (node is null) return Retry.TextboxNotFound("amount");

            Result result;
            result = await chromeManager.Input(By.XPath(node.XPath), amount.ToString());
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private static async ValueTask<Result> Confirm(IChromeManager chromeManager, CancellationToken cancellationToken)
        {
            var html = chromeManager.Html;
            var node = InventoryParser.GetConfirmButton(html);
            if (node is null) return Retry.ButtonNotFound("confirm");

            static bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryLoaded(doc);
            }

            Result result;
            result = await chromeManager.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeManager.Wait(driver => loadingCompleted(driver), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static long RoundUpTo100(long res)
        {
            if (res == 0) return 0;
            var remainder = res % 100;
            return res + (100 - remainder);
        }

        private static Result IsEnoughResource(
            AccountId accountId,
            long[] requiredResource,
            IDbContextFactory<AppDbContext> contextFactory)
        {
            using var context = contextFactory.CreateDbContext();
            var types = new List<HeroItemEnums>
            {
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

            var errors = new List<Error>();
            for (var i = 0; i < 4; i++)
            {
                var type = types[i];
                var item = items.Find(x => x.Type == type);
                var amount = item?.Amount ?? 0;
                if (amount < requiredResource[i])
                {
                    errors.Add(Resource.Error($"{type}", amount, requiredResource[i]));
                }
            }

            if (errors.Count > 0) return Result.Fail(errors);

            return Result.Ok();
        }
    }
}