using FluentResults;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;

namespace MainCore.Commands.General
{
    [RegisterAsTransient]
    public class CloseCommand : ICloseCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfRepository _unitOfRepository;

        public CloseCommand(IChromeManager chromeManager, IUnitOfRepository unitOfRepository)
        {
            _chromeManager = chromeManager;
            _unitOfRepository = unitOfRepository;
        }

        public Result Execute(AccountId accountId)
        {
            _unitOfRepository.AccountRepository.UpdateAccess(accountId);

            var chromeBrowser = _chromeManager.Get(accountId);
            chromeBrowser.Close();
            return Result.Ok();
        }
    }
}