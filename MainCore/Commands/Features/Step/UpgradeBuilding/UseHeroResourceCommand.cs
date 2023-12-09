using FluentResults;
using HtmlAgilityPack;
using MainCore.Commands.Base;
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
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly UnitOfCommand _unitOfCommand;
        private readonly IUnitOfParser _unitOfParser;

        public UseHeroResourceCommandHandler(IChromeManager chromeManager, IUnitOfRepository unitOfRepository, UnitOfCommand unitOfCommand, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfRepository = unitOfRepository;
            _unitOfCommand = unitOfCommand;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(UseHeroResourceCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);

            var currentUrl = chromeBrowser.CurrentUrl;
            Result result;
            result = await _unitOfCommand.ToHeroInventoryCommand.Handle(new(command.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateHeroItemsCommand.Handle(new(command.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

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
                result = await UseResource(chromeBrowser, item.Item1, item.Item2, cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.DelayClickCommand.Handle(new(command.AccountId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }

            result = await chromeBrowser.Navigate(currentUrl, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> UseResource(IChromeBrowser chromeBrowser, HeroItemEnums item, long amount, CancellationToken cancellationToken)
        {
            if (amount == 0) return Result.Ok();
            Result result;
            result = await ClickItem(chromeBrowser, item, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await EnterAmount(chromeBrowser, amount);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await Confirm(chromeBrowser, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> ClickItem(IChromeBrowser chromeBrowser, HeroItemEnums item, CancellationToken cancellationToken)
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

            result = await chromeBrowser.Wait(loadingCompleted, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> EnterAmount(IChromeBrowser chromeBrowser, long amount)
        {
            var html = chromeBrowser.Html;
            var node = _unitOfParser.HeroParser.GetAmountBox(html);
            if (node is null) return Result.Fail(Retry.TextboxNotFound("amount resource input"));
            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(node.XPath), amount.ToString());
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> Confirm(IChromeBrowser chromeBrowser, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var node = _unitOfParser.HeroParser.GetConfirmButton(html);
            if (node is null) return Result.Fail(Retry.ButtonNotFound("confirm use resource"));

            Result result;
            result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool loadingCompleted(IWebDriver driver)
            {
                var html = new HtmlDocument();
                html.LoadHtml(driver.PageSource);
                return !_unitOfParser.HeroParser.HeroInventoryLoading(html);
            };

            result = await chromeBrowser.Wait(loadingCompleted, cancellationToken);
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