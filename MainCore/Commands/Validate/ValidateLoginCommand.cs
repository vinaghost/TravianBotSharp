using FluentResults;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Services;

namespace MainCore.Commands.Validate
{
    [RegisterAsTransient]
    public class ValidateLoginCommand : IValidateLoginCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;

        public ValidateLoginCommand(IChromeManager chromeManager, IUnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public bool Value { get; private set; }

        public async Task<Result> Execute(AccountId accountId)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;

            var loginButton = _unitOfParser.LoginPageParser.GetLoginButton(html);

            Value = loginButton is not null;
            return Result.Ok();
        }
    }
}