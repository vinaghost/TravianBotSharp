using HtmlAgilityPack;

namespace MainCore.Commands.Misc
{
    public abstract class HeroInventoryCommandHandler
    {
        protected readonly IHeroParser _heroParser;
        protected readonly IHeroItemRepository _heroItemRepository;
        protected readonly IMediator _mediator;

        protected HeroInventoryCommandHandler(IHeroParser heroParser, IHeroItemRepository heroItemRepository, IMediator mediator)
        {
            _heroParser = heroParser;
            _heroItemRepository = heroItemRepository;
            _mediator = mediator;
        }

        public async Task<Result> ToHeroInventory(AccountId accountId, IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            Result result;
            result = await ToHeroInventory(chromeBrowser, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await UpdateHeroInventory(accountId, chromeBrowser, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> ToHeroInventory(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var avatar = _heroParser.GetHeroAvatar(html);
            if (avatar is null) return Retry.ButtonNotFound("avatar hero");

            Result result;
            result = await chromeBrowser.Click(By.XPath(avatar.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("hero", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool tabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return _heroParser.InventoryTabActive(doc);
            };

            result = await chromeBrowser.Wait(tabActived, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> UpdateHeroInventory(AccountId accountId, IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var dtos = _heroParser.GetItems(html);
            _heroItemRepository.Update(accountId, dtos.ToList());
            await _mediator.Publish(new HeroItemUpdated(accountId), cancellationToken);
            return Result.Ok();
        }
    }
}