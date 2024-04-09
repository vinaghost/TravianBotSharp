using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.General
{
    public class DelayClickCommand : ByAccountIdBase, IRequest
    {
        public DelayClickCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class DelayClickCommandHandler : IRequestHandler<DelayClickCommand>

    {
        private readonly UnitOfRepository _unitOfRepository;

        public DelayClickCommandHandler(UnitOfRepository unitOfRepository)
        {
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(DelayClickCommand command, CancellationToken cancellationToken)
        {
            var delay = _unitOfRepository.AccountSettingRepository.GetByName(command.AccountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);
            await Task.Delay(delay, CancellationToken.None);
        }
    }
}