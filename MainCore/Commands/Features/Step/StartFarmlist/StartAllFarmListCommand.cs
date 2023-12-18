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
    public class StartAllFarmListCommand : ByAccountIdBase, ICommand
    {
        public StartAllFarmListCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class StartAllFarmListCommandHandler : ICommandHandler<StartAllFarmListCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public StartAllFarmListCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(StartAllFarmListCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var startAllButton = _unitOfParser.FarmParser.GetStartAllButton(html);
            if (startAllButton is null) return Result.Fail(Retry.ButtonNotFound("Start all farms"));

            var result = await chromeBrowser.Click(By.XPath(startAllButton.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}