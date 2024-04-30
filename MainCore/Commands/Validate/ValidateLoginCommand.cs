using MainCore.Commands.Base;
using MainCore.Common.MediatR;

namespace MainCore.Commands.Validate
{
    public class ValidateLoginCommand : ByAccountIdBase, ICommand<bool>
    {
        public ValidateLoginCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class ValidateLoginCommandHandler : ICommandHandler<ValidateLoginCommand, bool>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public ValidateLoginCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        public bool Value { get; private set; }

        public async Task<Result> Handle(ValidateLoginCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var loginButton = _unitOfParser.LoginPageParser.GetLoginButton(html);

            Value = loginButton is not null;
            return Result.Ok();
        }
    }
}