using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Features.Step.StartFarmlist
{
    public class StartSingleFarmListCommand : ByAccountIdBase, ICommand
    {
        public FarmId FarmId { get; }

        public StartSingleFarmListCommand(AccountId accountId, FarmId farmId) : base(accountId)
        {
            FarmId = farmId;
        }
    }

    [RegisterAsTransient]
    public class StartSingleFarmListCommandHandler : ICommandHandler<StartSingleFarmListCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public StartSingleFarmListCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(StartSingleFarmListCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var startButton = _unitOfParser.FarmParser.GetStartButton(html, command.FarmId);
            if (startButton is null) return Result.Fail(Retry.ButtonNotFound($"Start farm {command.FarmId}"));

            var result = await chromeBrowser.Click(By.XPath(startButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}