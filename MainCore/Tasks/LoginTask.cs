using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public sealed class LoginTask : AccountTask
    {
        private readonly ILoginPageParser _loginPageParser;
        private readonly IOptionPageParser _optionPageParser;

        private readonly IAccountRepository _accountRepository;

        public LoginTask(IMediator mediator, ILoginPageParser loginPageParser, IAccountRepository accountRepository, IOptionPageParser optionPageParser) : base(mediator)
        {
            _loginPageParser = loginPageParser;
            _accountRepository = accountRepository;
            _optionPageParser = optionPageParser;
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await Login();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await DisableContextualHelp();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new UpdateVillageListCommand(AccountId, _chromeBrowser), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        public async Task<Result> Login()
        {
            var html = _chromeBrowser.Html;

            if (IsIngame()) return Result.Ok();

            var buttonNode = _loginPageParser.GetLoginButton(html);
            if (buttonNode is null) return Retry.ButtonNotFound("login");
            var usernameNode = _loginPageParser.GetUsernameNode(html);
            if (usernameNode is null) return Retry.TextboxNotFound("username");
            var passwordNode = _loginPageParser.GetPasswordNode(html);
            if (passwordNode is null) return Retry.TextboxNotFound("password");

            var username = _accountRepository.GetUsername(AccountId);
            var password = _accountRepository.GetPassword(AccountId);

            Result result;
            result = await _chromeBrowser.InputTextbox(By.XPath(usernameNode.XPath), username);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await _chromeBrowser.InputTextbox(By.XPath(passwordNode.XPath), password);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await _chromeBrowser.Click(By.XPath(buttonNode.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _chromeBrowser.WaitPageChanged("dorf", CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> DisableContextualHelp()
        {
            var contextualHelpEnable = IsContextualHelpEnable();
            if (!contextualHelpEnable) return Result.Ok();

            Result result;
            result = await ToOptionsPage();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await DisableOption();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await Submit();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await new ToDorfCommand().Execute(_chromeBrowser, 1, false, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private bool IsContextualHelpEnable()
        {
            var html = _chromeBrowser.Html;
            return _optionPageParser.IsContextualHelpShow(html);
        }

        private async Task<Result> ToOptionsPage()
        {
            var html = _chromeBrowser.Html;

            var button = _optionPageParser.GetOptionButton(html);
            Result result;
            result = await _chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> DisableOption()
        {
            var html = _chromeBrowser.Html;

            var option = _optionPageParser.GetHideContextualHelpOption(html);
            if (option is null) return Retry.NotFound("hide contextual help", "option");
            Result result;
            result = await _chromeBrowser.Click(By.XPath(option.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> Submit()
        {
            var html = _chromeBrowser.Html;

            var option = _optionPageParser.GetSubmitButton(html);
            Result result;
            result = await _chromeBrowser.Click(By.XPath(option.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private bool IsIngame()
        {
            var html = _chromeBrowser.Html;

            var serverTime = html.GetElementbyId("servertime");

            return serverTime is not null;
        }

        protected override void SetName()
        {
            _name = "Login task";
        }
    }
}