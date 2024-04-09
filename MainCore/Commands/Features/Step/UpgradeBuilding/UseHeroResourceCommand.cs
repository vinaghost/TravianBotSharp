using FluentResults;
using HtmlAgilityPack;
using MainCore.Commands.Base;
using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Features.Step.UpgradeBuilding
{
    public class UseHeroResourceCommand : ByAccountIdBase, ICommand
    {
        public long[] RequiredResource { get; }

        public UseHeroResourceCommand(AccountId accountId, long[] requiredResource) : base(accountId)
        {
            RequiredResource = requiredResource;
        }
    }

    [RegisterAsTransient]
    public class UseHeroResourceCommandHandler : ICommandHandler<UseHeroResourceCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfRepository _unitOfRepository;
        
        private readonly UnitOfParser _unitOfParser;

        public UseHeroResourceCommandHandler(IChromeManager chromeManager, UnitOfRepository unitOfRepository, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfRepository = unitOfRepository;
            
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(UseHeroResourceCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);

            var currentUrl = chromeBrowser.CurrentUrl;
            Result result;
            result = await _unitOfCommand.ToHeroInventoryCommand.Handle(new(command.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateHeroItemsCommand.Handle(new(command.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var requiredResource = command.RequiredResource;
            for (var i = 0; i < 4; i++)
            {
                requiredResource[i] = RoundUpTo100(requiredResource[i]);
            }

            result = _unitOfRepository.HeroItemRepository.IsEnoughResource(command.AccountId, requiredResource);
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

            var delayClickCommand = new DelayClickCommand(command.AccountId);
            foreach (var item in items)
            {
                result = await UseResource(chromeBrowser, item.Item1, item.Item2, delayClickCommand, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            result = await chromeBrowser.Navigate(currentUrl, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> UseResource(IChromeBrowser chromeBrowser, HeroItemEnums item, long amount, DelayClickCommand delayClickCommand, CancellationToken cancellationToken)
        {
            if (amount == 0) return Result.Ok();
            Result result;
            result = await ClickItem(chromeBrowser, item, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(delayClickCommand, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await EnterAmount(chromeBrowser, amount);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(delayClickCommand, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await Confirm(chromeBrowser, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(delayClickCommand, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> ClickItem(IChromeBrowser chromeBrowser, HeroItemEnums item, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var node = _unitOfParser.HeroParser.GetItemSlot(html, item);
            if (node is null) return Retry.NotFound($"{item}", "item");

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return !_unitOfParser.HeroParser.HeroInventoryLoading(doc);
            };

            result = await chromeBrowser.Wait(loadingCompleted, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> EnterAmount(IChromeBrowser chromeBrowser, long amount)
        {
            var html = chromeBrowser.Html;
            var node = _unitOfParser.HeroParser.GetAmountBox(html);
            if (node is null) return Retry.TextboxNotFound("amount resource input");
            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(node.XPath), amount.ToString());
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> Confirm(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var node = _unitOfParser.HeroParser.GetConfirmButton(html);
            if (node is null) return Retry.ButtonNotFound("confirm use resource");

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool loadingCompleted(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return !_unitOfParser.HeroParser.HeroInventoryLoading(doc);
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