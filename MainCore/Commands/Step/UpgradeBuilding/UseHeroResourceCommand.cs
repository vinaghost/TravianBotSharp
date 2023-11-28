using FluentResults;
using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    [RegisterAsTransient]
    public class UseHeroResourceCommand : IUseHeroResourceCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IUnitOfCommand _unitOfCommand;
        private readonly IUnitOfParser _unitOfParser;

        public UseHeroResourceCommand(IChromeManager chromeManager, IUnitOfRepository unitOfRepository, IUnitOfCommand unitOfCommand, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfRepository = unitOfRepository;
            _unitOfCommand = unitOfCommand;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Execute(AccountId accountId, long[] requiredResource)
        {
            var chromeBrowser = _chromeManager.Get(accountId);

            var currentUrl = chromeBrowser.CurrentUrl;
            Result result;
            result = await _unitOfCommand.ToHeroInventoryCommand.Execute(accountId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateHeroItemsCommand.Execute(accountId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            for (var i = 0; i < 4; i++)
            {
                requiredResource[i] = RoundUpTo100(requiredResource[i]);
            }

            result = _unitOfRepository.HeroItemRepository.IsEnoughResource(accountId, requiredResource);
            if (result.IsFailed)
            {
                if (!result.HasError<Retry>())
                {
                    var chromeResult = await chromeBrowser.Navigate(currentUrl);
                    if (chromeResult.IsFailed) return chromeResult.WithError(new TraceMessage(TraceMessage.Line()));
                }
                return result.WithError(new TraceMessage(TraceMessage.Line()));
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
                result = await UseResource(chromeBrowser, item.Item1, item.Item2);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.DelayClickCommand.Execute(accountId);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }

            result = await chromeBrowser.Navigate(currentUrl);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> UseResource(IChromeBrowser chromeBrowser, HeroItemEnums item, long amount)
        {
            if (amount == 0) return Result.Ok();
            Result result;
            result = await ClickItem(chromeBrowser, item);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await EnterAmount(chromeBrowser, amount);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await Confirm(chromeBrowser);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> ClickItem(IChromeBrowser chromeBrowser, HeroItemEnums item)
        {
            var html = chromeBrowser.Html;
            var node = _unitOfParser.HeroParser.GetItemSlot(html, item);
            if (node is null) return Result.Fail(Retry.NotFound($"{item}", "item"));

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool loadingCompleted(IWebDriver driver)
            {
                var html = new HtmlDocument();
                html.LoadHtml(driver.PageSource);
                return !_unitOfParser.HeroParser.HeroInventoryLoading(html);
            };

            result = await chromeBrowser.Wait(loadingCompleted);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> EnterAmount(IChromeBrowser chromeBrowser, long amount)
        {
            var html = chromeBrowser.Html;
            var node = _unitOfParser.HeroParser.GetAmountBox(html);
            if (node is null) return Result.Fail(Retry.TextboxNotFound("amount input"));
            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(node.XPath), amount.ToString());
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> Confirm(IChromeBrowser chromeBrowser)
        {
            var html = chromeBrowser.Html;
            var node = _unitOfParser.HeroParser.GetConfirmButton(html);
            if (node is null) return Result.Fail(Retry.ButtonNotFound("Confirm"));

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool loadingCompleted(IWebDriver driver)
            {
                var html = new HtmlDocument();
                html.LoadHtml(driver.PageSource);
                return !_unitOfParser.HeroParser.HeroInventoryLoading(html);
            };

            result = await chromeBrowser.Wait(loadingCompleted);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        private static long RoundUpTo100(long res)
        {
            var remainder = res % 100;
            return res + (100 - remainder);
        }
    }
}