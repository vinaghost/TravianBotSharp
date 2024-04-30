using HtmlAgilityPack;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;

namespace MainCore.Commands.Navigate
{
    public class ToHeroInventoryCommand : ByAccountIdBase, ICommand
    {
        public ToHeroInventoryCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class ToHeroInventoryCommandHandler : ICommandHandler<ToHeroInventoryCommand>
    {
        private readonly UnitOfParser _unitOfParser;
        private readonly IChromeManager _chromeManager;

        public ToHeroInventoryCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(ToHeroInventoryCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var avatar = _unitOfParser.HeroParser.GetHeroAvatar(html);
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
                return _unitOfParser.HeroParser.InventoryTabActive(doc);
            };

            result = await chromeBrowser.Wait(tabActived, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}