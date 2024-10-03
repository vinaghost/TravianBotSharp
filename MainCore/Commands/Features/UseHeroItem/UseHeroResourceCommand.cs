using MainCore.Commands.Abstract;
using MainCore.Common.Errors.Storage;

namespace MainCore.Commands.Features.UseHeroItem
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class UseHeroResourceCommand(DataService dataService, IDbContextFactory<AppDbContext> contextFactory, DelayClickCommand delayClickCommand) : CommandBase(dataService), ICommand<long[]>
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;
        private readonly DelayClickCommand _delayClickCommand = delayClickCommand;

        public async Task<Result> Execute(long[] resource, CancellationToken cancellationToken)
        {
            for (var i = 0; i < 4; i++)
            {
                resource[i] = RoundUpTo100(resource[i]);
            }

            Result result;
            result = IsEnoughResource(resource);
            if (result.IsFailed) return result;

            var items = new Dictionary<HeroItemEnums, long>()
            {
                { HeroItemEnums.Wood, resource[0]},
                { HeroItemEnums.Clay, resource[1]},
                { HeroItemEnums.Iron, resource[2]},
                { HeroItemEnums.Crop, resource[3]},
            };

            foreach (var item in items)
            {
                result = await UseResource(item.Key, item.Value, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            return Result.Ok();
        }

        private async Task<Result> UseResource(HeroItemEnums item, long amount, CancellationToken cancellationToken)
        {
            if (amount == 0) return Result.Ok();
            Result result;
            result = await ClickItem(item, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _delayClickCommand.Execute(cancellationToken);

            result = await EnterAmount(amount);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _delayClickCommand.Execute(cancellationToken);

            result = await Confirm(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _delayClickCommand.Execute(cancellationToken);

            return Result.Ok();
        }

        private async Task<Result> ClickItem(HeroItemEnums item, CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var node = InventoryParser.GetItemSlot(html, item);
            if (node is null) return Retry.NotFound($"{item}", "item");

            static bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryLoaded(doc);
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath), loadingCompleted, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> EnterAmount(long amount)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var node = InventoryParser.GetAmountBox(html);
            if (node is null) return Retry.TextboxNotFound("amount");

            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(node.XPath), amount.ToString());
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> Confirm(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var node = InventoryParser.GetConfirmButton(html);
            if (node is null) return Retry.ButtonNotFound("confirm");

            static bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return InventoryParser.IsInventoryLoaded(doc);
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

        private Result IsEnoughResource(long[] requiredResource)
        {
            var accountId = _dataService.AccountId;
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