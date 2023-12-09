using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Common.Models;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;

namespace MainCore.Commands.General
{
    public class OpenBrowserCommand : ByAccountIdBase, ICommand
    {
        public AccessDto Access { get; }

        public OpenBrowserCommand(AccountId accountId, AccessDto access) : base(accountId)
        {
            Access = access;
        }
    }

    [RegisterAsTransient]
    public class OpenBrowserCommandHandler : ICommandHandler<OpenBrowserCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfRepository _unitOfRepository;

        public OpenBrowserCommandHandler(IChromeManager chromeManager, IUnitOfRepository unitOfRepository)
        {
            _chromeManager = chromeManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task<Result> Handle(OpenBrowserCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);

            var account = _unitOfRepository.AccountRepository.Get(command.AccountId);

            var serverFolderName = account.Server.Replace("https://", "").Replace("http://", "").Replace(".", "_");
            var accountFolderName = account.Username;

            var headlessChrome = _unitOfRepository.AccountSettingRepository.GetBooleanByName(command.AccountId, Common.Enums.AccountSettingEnums.HeadlessChrome);
            var profilePath = Path.Combine(serverFolderName, accountFolderName);
            var chromeSetting = new ChromeSetting()
            {
                UserAgent = command.Access.Useragent,
                ProfilePath = profilePath,
                ProxyHost = command.Access.ProxyHost,
                ProxyPort = command.Access.ProxyPort,
                ProxyUsername = command.Access.ProxyUsername,
                ProxyPassword = command.Access.ProxyPassword,
                IsHeadless = headlessChrome,
            };
            var result = await chromeBrowser.Setup(chromeSetting);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.Navigate($"{account.Server}dorf1.php", cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}