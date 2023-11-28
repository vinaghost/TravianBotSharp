using FluentResults;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Step.TrainTroop
{
    [RegisterAsTransient]
    public class InputAmountTroopCommand : IInputAmountTroopCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public InputAmountTroopCommand(IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Execute(AccountId accountId, TroopEnums troop, int amount)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;

            var inputBox = _unitOfParser.TroopPageParser.GetInputBox(html, troop);
            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(inputBox.XPath), $"{amount}");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var trainButton = _unitOfParser.TroopPageParser.GetTrainButton(html);
            result = await chromeBrowser.Click(By.XPath(trainButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}