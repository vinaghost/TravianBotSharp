using FluentResults;
using MainCore.Common.Errors;
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
            var result = await Task.Run(() => chromeBrowser.Setup(account, access));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}