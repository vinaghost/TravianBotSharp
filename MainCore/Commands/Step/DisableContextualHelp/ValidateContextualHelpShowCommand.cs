using FluentResults;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;

namespace MainCore.Commands.Step.DisableContextualHelp
{
    [RegisterAsTransient]
    public class ValidateContextualHelpCommand : IValidateContextualHelpCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public bool Value { get; private set; }

        public ValidateContextualHelpCommand(IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public async Task<Result> Execute(AccountId accountId)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;

            Value = _unitOfParser.OptionPageParser.IsContextualHelpShow(html);

            return Result.Ok();
        }
    }
}