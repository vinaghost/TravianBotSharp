using FluentResults;
using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;

namespace MainCore.Commands.General
{
    [RegisterAsTransient]
    public class DelayTaskCommand : IDelayTaskCommand
    {
        private readonly IUnitOfRepository _unitOfRepository;

        public DelayTaskCommand(IUnitOfRepository unitOfRepository)
        {
            _unitOfRepository = unitOfRepository;
        }

        public async Task<Result> Execute(AccountId accountId)
        {
            var delay = _unitOfRepository.AccountSettingRepository.GetByName(accountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);
            await Task.Delay(delay);
            return Result.Ok();
        }
    }
}