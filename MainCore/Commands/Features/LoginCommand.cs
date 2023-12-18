using FluentResults;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;
using OpenQA.Selenium;

namespace MainCore.Commands.Features
{
    public class LoginCommand : ByAccountIdBase, IRequest<Result>
    {
        public LoginCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;
        private readonly UnitOfRepository _unitOfRepository;

        public LoginCommandHandler(IChromeManager chromeManager, UnitOfParser unitOfParser, UnitOfRepository unitOfRepository)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _unitOfRepository = unitOfRepository;
        }

        public async Task<Result> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = request.AccountId;

            var chromeBrowser = _chromeManager.Get(accountId);

            var html = chromeBrowser.Html;

            var resourceButton = _unitOfParser.NavigationBarParser.GetResourceButton(html);
            if (resourceButton is not null) return Result.Ok();

            var buttonNode = _unitOfParser.LoginPageParser.GetLoginButton(html);
            if (buttonNode is null) return Retry.ButtonNotFound("login");
            var usernameNode = _unitOfParser.LoginPageParser.GetUsernameNode(html);
            if (usernameNode is null) return Retry.TextboxNotFound("username");
            var passwordNode = _unitOfParser.LoginPageParser.GetPasswordNode(html);
            if (passwordNode is null) return Retry.TextboxNotFound("password");

            var username = _unitOfRepository.AccountRepository.GetUsername(accountId);
            var password = _unitOfRepository.AccountRepository.GetPassword(accountId);

            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(usernameNode.XPath), username);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await chromeBrowser.InputTextbox(By.XPath(passwordNode.XPath), password);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await chromeBrowser.Click(By.XPath(buttonNode.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageChanged("dorf", cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}