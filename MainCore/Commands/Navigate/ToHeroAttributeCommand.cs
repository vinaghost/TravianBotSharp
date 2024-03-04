using FluentResults;
using HtmlAgilityPack;
using MainCore.Commands.Base;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Navigate
{
    public class ToHeroAttributeCommand : ByAccountIdBase, ICommand
    {
        public ToHeroAttributeCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class ToHeroAttributeCommandHandler : ICommandHandler<ToHeroAttributeCommand>
    {
        private readonly UnitOfParser _unitOfParser;
        private readonly IChromeManager _chromeManager;

        public ToHeroAttributeCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(ToHeroAttributeCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var avatar = _unitOfParser.HeroParser.GetHeroAvatar(html);
            if (avatar is null) return Result.Fail(Retry.ButtonNotFound("avatar hero"));

            Result result;
            result = await chromeBrowser.Click(By.XPath(avatar.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("hero", cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool inventoryTabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return _unitOfParser.HeroParser.InventoryTabActive(doc);
            };

            result = await chromeBrowser.Wait(inventoryTabActived, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            html = chromeBrowser.Html;
            var attributeTab = _unitOfParser.HeroParser.GetHeroAttributeNode(html);
            if (attributeTab is null) return Result.Fail(Retry.ButtonNotFound("hero attribute"));

            result = await chromeBrowser.Click(By.XPath(attributeTab.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("attributes", cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool attributeTabActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return _unitOfParser.HeroParser.AttributeTabActive(doc);
            };

            result = await chromeBrowser.Wait(attributeTabActived, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}