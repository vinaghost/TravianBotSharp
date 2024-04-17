using FluentResults;
using MainCore.Commands;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;
using OpenQA.Selenium;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public sealed class LoginTask : AccountTask
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public LoginTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, IChromeManager chromeManager, UnitOfParser unitOfParser) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await Login();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await DisableContextualHelp();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateVillageListCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        public async Task<Result> Login()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);

            var html = chromeBrowser.Html;

            var resourceButton = _unitOfParser.NavigationBarParser.GetResourceButton(html);
            if (resourceButton is not null) return Result.Ok();

            var buttonNode = _unitOfParser.LoginPageParser.GetLoginButton(html);
            if (buttonNode is null) return Retry.ButtonNotFound("login");
            var usernameNode = _unitOfParser.LoginPageParser.GetUsernameNode(html);
            if (usernameNode is null) return Retry.TextboxNotFound("username");
            var passwordNode = _unitOfParser.LoginPageParser.GetPasswordNode(html);
            if (passwordNode is null) return Retry.TextboxNotFound("password");

            var username = _unitOfRepository.AccountRepository.GetUsername(AccountId);
            var password = _unitOfRepository.AccountRepository.GetPassword(AccountId);

            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(usernameNode.XPath), username);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await chromeBrowser.InputTextbox(By.XPath(passwordNode.XPath), password);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await chromeBrowser.Click(By.XPath(buttonNode.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("dorf", CancellationToken);
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

            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 1), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private bool IsContextualHelpEnable()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;
            return _unitOfParser.OptionPageParser.IsContextualHelpShow(html);
        }

        private async Task<Result> ToOptionsPage()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var button = _unitOfParser.OptionPageParser.GetOptionButton(html);
            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> DisableOption()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var option = _unitOfParser.OptionPageParser.GetHideContextualHelpOption(html);
            if (option is null) return Retry.NotFound("hide contextual help", "option");
            Result result;
            result = await chromeBrowser.Click(By.XPath(option.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> Submit()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var option = _unitOfParser.OptionPageParser.GetSubmitButton(html);
            Result result;
            result = await chromeBrowser.Click(By.XPath(option.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            _name = "Login task";
        }
    }
}