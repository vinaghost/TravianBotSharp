using FluentResults;
using MainCore.Common.Errors;
using MainCore.Common.Models;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;

namespace MainCore.Commands.General
{
    [RegisterAsTransient]
    public class OpenBrowserCommand : IOpenBrowserCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfRepository _unitOfRepository;

        public OpenBrowserCommand(IChromeManager chromeManager, IUnitOfRepository unitOfRepository)
        {
            _chromeManager = chromeManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task<Result> Execute(AccountId accountId, AccessDto access)
        {
            var chromeBrowser = _chromeManager.Get(accountId);

            var account = _unitOfRepository.AccountRepository.Get(accountId);

            var serverFolderName = account.Server.Replace("https://", "").Replace("http://", "").Replace(".", "_");
            var accountFolderName = account.Username;

            var headlessChrome = _unitOfRepository.AccountSettingRepository.GetBooleanByName(accountId, Common.Enums.AccountSettingEnums.HeadlessChrome);
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
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.Navigate($"{account.Server}dorf1.php");
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}