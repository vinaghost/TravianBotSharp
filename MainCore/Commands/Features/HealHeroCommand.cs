using FluentResults;
using HtmlAgilityPack;
using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;
using OpenQA.Selenium;

namespace MainCore.Commands.Features
{
    public class HealHeroCommand : ByAccountIdBase, IRequest<Result>
    {
        public HealHeroCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class HealHeroCommandHandler : IRequestHandler<HealHeroCommand, Result>
    {
        private readonly UnitOfCommand _unitOfCommand;
        private readonly UnitOfParser _unitOfParser;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IChromeManager _chromeManager;
        private readonly ILogService _logService;

        public HealHeroCommandHandler(UnitOfCommand unitOfCommand, IChromeManager chromeManager, UnitOfRepository unitOfRepository, UnitOfParser unitOfParser, ILogService logService)
        {
            _unitOfCommand = unitOfCommand;
            _chromeManager = chromeManager;
            _unitOfRepository = unitOfRepository;
            _unitOfParser = unitOfParser;
            _logService = logService;
        }

        public async Task<Result> Handle(HealHeroCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var currentHealth = _unitOfRepository.HeroRepository.GetHealth(accountId);

            var requiredHealth = _unitOfRepository.AccountSettingRepository.GetByName(accountId, AccountSettingEnums.HealthBeforeStartAdventure);

            Result result;
            result = await _unitOfCommand.ToHeroInventoryCommand.Handle(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateHeroItemsCommand.Handle(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var requiredOintment = requiredHealth - currentHealth;
            var isEnoughOintment = _unitOfRepository.HeroItemRepository.IsEnoughOintment(accountId, requiredOintment);
            if (!isEnoughOintment) return Result.Fail(new Skip($"Hero doesn't have enough ointment to heal"));

            var logger = _logService.GetLogger(accountId);
            logger.Information("Will use {requiredOintment} ointment to healing hero", requiredOintment, currentHealth);

            var chromeBrowser = _chromeManager.Get(accountId);
            var delayClickCommand = new DelayClickCommand(accountId);

            result = await ClickItem(chromeBrowser, HeroItemEnums.Ointment, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(delayClickCommand, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await EnterAmount(chromeBrowser, requiredOintment);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(delayClickCommand, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await Confirm(chromeBrowser, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(delayClickCommand, cancellationToken);
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
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return !_unitOfParser.HeroParser.HeroInventoryLoading(doc);
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
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return !_unitOfParser.HeroParser.HeroInventoryLoading(doc);
            };

            result = await chromeBrowser.Wait(loadingCompleted, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}