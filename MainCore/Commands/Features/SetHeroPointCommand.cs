using FluentResults;
using HtmlAgilityPack;
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
    public class SetHeroPointCommand : ByAccountIdBase, IRequest<Result>
    {
        public SetHeroPointCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class SetHeroPointCommandHandler : IRequestHandler<SetHeroPointCommand, Result>
    {
        private readonly UnitOfParser _unitOfParser;
        private readonly UnitOfCommand _unitOfCommand;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IChromeManager _chromeManager;

        public SetHeroPointCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser, UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _unitOfCommand = unitOfCommand;
            _unitOfRepository = unitOfRepository;
        }

        public async Task<Result> Handle(SetHeroPointCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;

            var fightingInput = _unitOfParser.HeroParser.GetFightingStrengthInputBox(html);
            if (fightingInput is null)
            {
                return Result.Fail(Retry.TextboxNotFound("fighting strength"));
            }
            var offInput = _unitOfParser.HeroParser.GetOffBonusInputBox(html);
            if (offInput is null)
            {
                return Result.Fail(Retry.TextboxNotFound("off bonus"));
            }
            var defInput = _unitOfParser.HeroParser.GetDefBonusInputBox(html);
            if (defInput is null)
            {
                return Result.Fail(Retry.TextboxNotFound("def bonus"));
            }
            var resourceInput = _unitOfParser.HeroParser.GetResourceProductionInputBox(html);
            if (resourceInput is null)
            {
                return Result.Fail(Retry.TextboxNotFound("resource production"));
            }

            var saveButton = _unitOfParser.HeroParser.GetSaveButton(html);
            if (saveButton is null)
            {
                return Result.Fail(Retry.ButtonNotFound("save"));
            }

            var currentFightingPoint = fightingInput.GetAttributeValue("value", 0);
            var currentOffPoint = offInput.GetAttributeValue("value", 0);
            var currentDefPoint = defInput.GetAttributeValue("value", 0);
            var currentResourcePoint = resourceInput.GetAttributeValue("value", 0);

            var heroFightingPoint = _unitOfRepository.AccountSettingRepository.GetByName(accountId, AccountSettingEnums.HeroFightingPoint);
            var heroOffPoint = _unitOfRepository.AccountSettingRepository.GetByName(accountId, AccountSettingEnums.HeroOffPoint);
            var heroDefPoint = _unitOfRepository.AccountSettingRepository.GetByName(accountId, AccountSettingEnums.HeroDefPoint);
            var heroResourcePoint = _unitOfRepository.AccountSettingRepository.GetByName(accountId, AccountSettingEnums.HeroResourcePoint);

            currentFightingPoint += heroFightingPoint;
            currentOffPoint += heroOffPoint;
            currentDefPoint += heroDefPoint;
            currentResourcePoint += heroResourcePoint;

            var result = await chromeBrowser.InputTextbox(By.XPath(fightingInput.XPath), $"{currentFightingPoint}");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.InputTextbox(By.XPath(offInput.XPath), $"{currentOffPoint}");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.InputTextbox(By.XPath(defInput.XPath), $"{currentDefPoint}");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.InputTextbox(By.XPath(resourceInput.XPath), $"{currentResourcePoint}");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool saveButtonActived(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                saveButton = _unitOfParser.HeroParser.GetSaveButton(doc);
                var element = driver.FindElement(By.XPath(saveButton.XPath));
                return element.Displayed && element.Enabled;
            };

            result = await chromeBrowser.Wait(saveButtonActived, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.Click(By.XPath(saveButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}