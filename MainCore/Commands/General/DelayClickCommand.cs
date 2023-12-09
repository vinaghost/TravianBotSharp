using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;

namespace MainCore.Commands.General
{
    public class DelayClickCommand : ByAccountIdBase, ICommand
    {
        public DelayClickCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class DelayClickCommandHandler : ICommandHandler<DelayClickCommand>

    {
        private readonly UnitOfRepository _unitOfRepository;

        public DelayClickCommandHandler(UnitOfRepository unitOfRepository)
        {
            _unitOfRepository = unitOfRepository;
        }

        public async Task<Result> Handle(DelayClickCommand command, CancellationToken cancellationToken)
        {
            var delay = _unitOfRepository.AccountSettingRepository.GetByName(command.AccountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);
            await Task.Delay(delay, CancellationToken.None);
            return Result.Ok();
        }
    }
}