using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;

namespace MainCore.Commands.General
{
    public class CloseBrowserCommand : ByAccountIdBase, ICommand
    {
        public CloseBrowserCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class CloseBrowserCommandHandler : ICommandHandler<CloseBrowserCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfRepository _unitOfRepository;

        public CloseBrowserCommandHandler(IChromeManager chromeManager, IUnitOfRepository unitOfRepository)
        {
            _chromeManager = chromeManager;
            _unitOfRepository = unitOfRepository;
        }

        public async Task<Result> Handle(CloseBrowserCommand command, CancellationToken cancellationToken)
        {
            _unitOfRepository.AccountRepository.UpdateAccess(command.AccountId);

            var chromeBrowser = _chromeManager.Get(command.AccountId);
            await Task.Run(chromeBrowser.Close, CancellationToken.None);
            return Result.Ok();
        }
    }
}