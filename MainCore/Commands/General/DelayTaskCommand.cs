using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.General
{
    public class DelayTaskCommand : ByAccountIdBase, IRequest
    {
        public DelayTaskCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class DelayTaskCommandHandler : IRequestHandler<DelayTaskCommand>
    {
        private readonly UnitOfRepository _unitOfRepository;

        public DelayTaskCommandHandler(UnitOfRepository unitOfRepository)
        {
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(DelayTaskCommand command, CancellationToken cancellationToken)
        {
            var delay = _unitOfRepository.AccountSettingRepository.GetByName(command.AccountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);
            await Task.Delay(delay, CancellationToken.None);
        }
    }
}