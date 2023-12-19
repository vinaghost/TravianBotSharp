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
    public class OptionsSubmitCommand : ByAccountIdBase, ICommand
    {
        public OptionsSubmitCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class OptionsSubmitCommandHandler : ICommandHandler<OptionsSubmitCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public OptionsSubmitCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Handle(OptionsSubmitCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var option = _unitOfParser.OptionPageParser.GetSubmitButton(html);
            Result result;
            result = await chromeBrowser.Click(By.XPath(option.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}