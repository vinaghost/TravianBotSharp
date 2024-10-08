﻿using MainCore.Commands.Abstract;
using MainCore.Common.Models;

namespace MainCore.Commands.Misc
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class OpenBrowserCommand(DataService dataService) : CommandBase(dataService), ICommand<AccessDto>
    {
        public async Task<Result> Execute(AccessDto access, CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var accountId = _dataService.AccountId;

            var account = new GetAccount().Execute(accountId);
            var uri = new Uri(account.Server);

            var serverFolderName = uri.Host.Replace(".", "_");
            var accountFolderName = account.Username;

            var headlessChrome = new GetSetting().BooleanByName(accountId, AccountSettingEnums.HeadlessChrome);
            var profilePath = Path.Combine(serverFolderName, accountFolderName);
            var chromeSetting = new ChromeSetting()
            {
                UserAgent = access.Useragent,
                ProfilePath = profilePath,
                ProxyHost = access.ProxyHost,
                ProxyPort = access.ProxyPort,
                ProxyUsername = access.ProxyUsername,
                ProxyPassword = access.ProxyPassword,
                IsHeadless = headlessChrome,
            };
            var result = await chromeBrowser.Setup(chromeSetting);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.Navigate($"{account.Server}/dorf1.php", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}