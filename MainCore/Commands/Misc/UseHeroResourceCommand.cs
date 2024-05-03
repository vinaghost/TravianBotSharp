using HtmlAgilityPack;

namespace MainCore.Commands.Misc
{
    public class UseHeroResourceCommand : ByAccountIdBase, ICommand
    {
        public long[] RequiredResource { get; }
        public IChromeBrowser ChromeBrowser { get; }

        public UseHeroResourceCommand(AccountId accountId, IChromeBrowser chromeBrowser, long[] requiredResource) : base(accountId)
        {
            RequiredResource = requiredResource;
            ChromeBrowser = chromeBrowser;
        }
    }

    [RegisterAsTransient]
    public class UseHeroResourceCommandHandler : HeroInventoryCommandHandler, ICommandHandler<UseHeroResourceCommand>
    {
        private readonly DelayClickCommand _delayClickCommand;

        public UseHeroResourceCommandHandler(IHeroParser heroParser, IHeroItemRepository heroItemRepository, IMediator mediator, DelayClickCommand delayClickCommand) : base(heroParser, heroItemRepository, mediator)
        {
            _delayClickCommand = delayClickCommand;
        }

        public async Task<Result> Handle(UseHeroResourceCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var chromeBrowser = request.ChromeBrowser;
            var requiredResource = request.RequiredResource;

            var currentUrl = chromeBrowser.CurrentUrl;
            Result result;
            result = await ToHeroInventory(accountId, chromeBrowser, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            for (var i = 0; i < 4; i++)
            {
                requiredResource[i] = RoundUpTo100(requiredResource[i]);
            }

            result = _heroItemRepository.IsEnoughResource(accountId, requiredResource);
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

            await _delayClickCommand.Execute(accountId);

            result = await EnterAmount(chromeBrowser, amount);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _delayClickCommand.Execute(accountId);

            result = await Confirm(chromeBrowser, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _delayClickCommand.Execute(accountId);

            return Result.Ok();
        }

        private async Task<Result> ClickItem(IChromeBrowser chromeBrowser, HeroItemEnums item, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var node = _heroParser.GetItemSlot(html, item);
            if (node is null) return Retry.NotFound($"{item}", "item");

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return !_heroParser.HeroInventoryLoading(doc);
            };

            result = await chromeBrowser.Wait(loadingCompleted, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> EnterAmount(IChromeBrowser chromeBrowser, long amount)
        {
            var html = chromeBrowser.Html;
            var node = _heroParser.GetAmountBox(html);
            if (node is null) return Retry.TextboxNotFound("amount resource input");
            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(node.XPath), amount.ToString());
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> Confirm(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var node = _heroParser.GetConfirmButton(html);
            if (node is null) return Retry.ButtonNotFound("confirm use resource");

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return !_heroParser.HeroInventoryLoading(doc);
            };

            result = await chromeBrowser.Wait(loadingCompleted, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private static long RoundUpTo100(long res)
        {
            if (res == 0) return 0;
            var remainder = res % 100;
            return res + (100 - remainder);
        }
    }
}