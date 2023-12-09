using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;

namespace MainCore.Commands.General
{
    public class DelayTaskCommand : ByAccountIdBase, ICommand
    {
        public DelayTaskCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class DelayTaskCommandHandler : ICommandHandler<DelayTaskCommand>
    {
        private readonly IUnitOfRepository _unitOfRepository;

        public DelayTaskCommandHandler(IUnitOfRepository unitOfRepository)
        {
            _unitOfRepository = unitOfRepository;
        }

        public async Task<Result> Handle(DelayTaskCommand command, CancellationToken cancellationToken)
        {
            var delay = _unitOfRepository.AccountSettingRepository.GetByName(command.AccountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);
            await Task.Delay(delay, CancellationToken.None);
            return Result.Ok();
        }
    }
}