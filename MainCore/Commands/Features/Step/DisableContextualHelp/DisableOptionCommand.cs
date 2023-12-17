using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;
using OpenQA.Selenium;

namespace MainCore.Commands.Features.Step.DisableContextualHelp
{
    public class DisableOptionCommand : ByAccountIdBase, ICommand
    {
        public DisableOptionCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class DisableOptionCommandHandler : ICommandHandler<DisableOptionCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public DisableOptionCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(DisableOptionCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var option = _unitOfParser.OptionPageParser.GetHideContextualHelpOption(html);
            if (option is null) return Result.Fail(Retry.NotFound("hide contextual help", "option"));
            Result result;
            result = await chromeBrowser.Click(By.XPath(option.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}