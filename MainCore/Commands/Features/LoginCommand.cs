﻿using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features
{
    [RegisterScoped<LoginCommand>]
    public class LoginCommand(IDataService dataService, IDbContextFactory<AppDbContext> contextFactory) : CommandBase(dataService), ICommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var chromeBrowser = _dataService.ChromeBrowser;

            var html = chromeBrowser.Html;
            if (LoginParser.IsIngamePage(html)) return Result.Ok();

            var buttonNode = LoginParser.GetLoginButton(html);
            if (buttonNode is null) return Retry.ButtonNotFound("login");
            var usernameNode = LoginParser.GetUsernameInput(html);
            if (usernameNode is null) return Retry.TextboxNotFound("username");
            var passwordNode = LoginParser.GetPasswordInput(html);
            if (passwordNode is null) return Retry.TextboxNotFound("password");

            var (username, password) = GetLoginInfo(accountId);

            Result result;
            result = await chromeBrowser.Input(By.XPath(usernameNode.XPath), username);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await chromeBrowser.Input(By.XPath(passwordNode.XPath), password);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await chromeBrowser.Click(By.XPath(buttonNode.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await chromeBrowser.WaitPageChanged("dorf", cancellationToken);

            return Result.Ok();
        }

        private (string username, string password) GetLoginInfo(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var data = context.Accesses
                .Where(x => x.AccountId == accountId.Value)
                .OrderByDescending(x => x.LastUsed)
                .Select(x => new { x.Username, x.Password })
                .FirstOrDefault();

            if (data is null) return ("", "");

            return (data.Username, data.Password);
        }
    }
}