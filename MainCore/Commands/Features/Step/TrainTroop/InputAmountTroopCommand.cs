using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Features.Step.TrainTroop
{
    public class InputAmountTroopCommand : ByAccountIdBase, ICommand
    {
        public TroopEnums Troop { get; }
        public int Amount { get; }

        public InputAmountTroopCommand(AccountId accountId, TroopEnums troop, int amount) : base(accountId)
        {
            Troop = troop;
            Amount = amount;
        }
    }

    [RegisterAsTransient]
    public class InputAmountTroopCommandHandler : ICommandHandler<InputAmountTroopCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public InputAmountTroopCommandHandler(IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(InputAmountTroopCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var inputBox = _unitOfParser.TroopPageParser.GetInputBox(html, command.Troop);
            if (inputBox is null) return Result.Fail(Retry.TextboxNotFound("troop amount input"));
            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(inputBox.XPath), $"{command.Amount}");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var trainButton = _unitOfParser.TroopPageParser.GetTrainButton(html);
            if (trainButton is null) return Result.Fail(Retry.ButtonNotFound("train troop"));

            result = await chromeBrowser.Click(By.XPath(trainButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}