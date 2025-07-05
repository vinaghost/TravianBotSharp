using MainCore.Constraints;

namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public static partial class UseHeroResourceCommand
    {
        public sealed record Command(AccountId AccountId, long[] Resource) : IAccountCommand;

        private static readonly List<HeroItemEnums> ResourceItemTypes = new()
        {
            HeroItemEnums.Wood,
            HeroItemEnums.Clay,
            HeroItemEnums.Iron,
            HeroItemEnums.Crop,
        };

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ILogger logger,
            AppDbContext context,
            DelayClickCommand.Handler delayClickCommand,
            CancellationToken cancellationToken)
        {
            var (accountId, resource) = command;

            var resourceItems = context.HeroItems
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => ResourceItemTypes.Contains(x.Type))
                .OrderBy(x => x.Type)
                .ToList();

            resource = resource.Select(RoundUpTo100).ToArray();

            var result = IsEnoughResource(resourceItems, resource);
            if (result.IsFailed) return result;

            var items = new Dictionary<HeroItemEnums, long>
            {
                { HeroItemEnums.Wood, resource[0] },
                { HeroItemEnums.Clay, resource[1] },
                { HeroItemEnums.Iron, resource[2] },
                { HeroItemEnums.Crop, resource[3] },
            };

            foreach (var (item, amount) in items)
            {
                if (amount == 0) continue;

                logger.Information($"Use {amount} {item} from hero inventory");

                result = await ClickItem(item, browser, cancellationToken);
                if (result.IsFailed) return result;

                await delayClickCommand.HandleAsync(new(accountId), cancellationToken);

                result = await EnterAmount(amount, browser, cancellationToken);
                if (result.IsFailed) return result;

                await delayClickCommand.HandleAsync(new(accountId), cancellationToken);

                result = await Confirm(browser, cancellationToken);
                if (result.IsFailed) return result;

                await delayClickCommand.HandleAsync(new(accountId), cancellationToken);
            }

            return Result.Ok();
        }

        private static async Task<Result> ClickItem(
            HeroItemEnums item,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var html = browser.Html;
            var node = InventoryParser.GetItemSlot(html, item);
            if (node is null) return Retry.NotFound($"{item}", "item");

            static bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryLoaded(doc);
            }

            Result result;
            result = await browser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result;

            result = await browser.Wait(driver => loadingCompleted(driver), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static async Task<Result> EnterAmount(
            long amount,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var html = browser.Html;
            var node = InventoryParser.GetAmountBox(html);
            if (node is null) return Retry.TextboxNotFound("amount");

            Result result;
            result = await browser.Input(By.XPath(node.XPath), amount.ToString());
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private static async Task<Result> Confirm(
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var html = browser.Html;
            var node = InventoryParser.GetConfirmButton(html);
            if (node is null) return Retry.ButtonNotFound("confirm");

            static bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryLoaded(doc);
            }

            Result result;
            result = await browser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result;

            result = await browser.Wait(driver => loadingCompleted(driver), cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }

        private static long RoundUpTo100(long res)
        {
            if (res == 0) return 0;
            var remainder = res % 100;
            return res + (100 - remainder);
        }

        private static Result IsEnoughResource(
            List<HeroItem> items,
            long[] requiredResource)
        {
            var errors = new List<Error>();
            for (var i = 0; i < 4; i++)
            {
                var type = ResourceItemTypes[i];
                var item = items.Find(x => x.Type == type);
                var amount = item?.Amount ?? 0;
                if (amount < requiredResource[i])
                {
                    errors.Add(ResourceMissing.Error($"{type}", amount, requiredResource[i]));
                }
            }

            if (errors.Count > 0) return Result.Fail(errors);

            return Result.Ok();
        }
    }
}