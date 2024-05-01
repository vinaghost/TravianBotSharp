using MainCore.Common.Models;
using MainCore.DTO;

namespace MainCore.Commands.Misc
{
    public class OpenBrowserCommand : ByAccountIdBase, ICommand
    {
        public AccessDto Access { get; }
        public IChromeBrowser ChromeBrowser { get; }

        public OpenBrowserCommand(AccountId accountId, AccessDto access, IChromeBrowser chromeBrowser) : base(accountId)
        {
            Access = access;
            ChromeBrowser = chromeBrowser;
        }
    }

    public class OpenBrowserCommandHandler : ICommandHandler<OpenBrowserCommand>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAccountSettingRepository _accountSettingRepository;

        public OpenBrowserCommandHandler(IAccountRepository accountRepository, IAccountSettingRepository accountSettingRepository)
        {
            _accountRepository = accountRepository;
            _accountSettingRepository = accountSettingRepository;
        }

        public async Task<Result> Handle(OpenBrowserCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var chromeBrowser = request.ChromeBrowser;
            var access = request.Access;

            var account = _accountRepository.Get(accountId);

            var serverFolderName = account.Server.Replace("https://", "").Replace("http://", "").Replace(".", "_");
            var accountFolderName = account.Username;

            var headlessChrome = _accountSettingRepository.GetBooleanByName(accountId, AccountSettingEnums.HeadlessChrome);
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

            result = await chromeBrowser.Navigate($"{account.Server}dorf1.php", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}