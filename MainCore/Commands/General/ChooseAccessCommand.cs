using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using Serilog;

namespace MainCore.Commands.General
{
    public class ChooseAccessCommand : ByAccountIdBase, ICommand<AccessDto>
    {
        public bool IgnoreSleepTime { get; }

        public ChooseAccessCommand(AccountId accountId, bool ignoreSleepTime) : base(accountId)
        {
            IgnoreSleepTime = ignoreSleepTime;
        }
    }

    [RegisterAsTransient]
    public class ChooseAccessCommandHandler : ICommandHandler<ChooseAccessCommand, AccessDto>
    {
        public AccessDto Value { get; private set; }

        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IUnitOfCommand _unitOfCommand;
        private readonly ILogService _logService;

        public ChooseAccessCommandHandler(IUnitOfRepository unitOfRepository, IUnitOfCommand unitOfCommand, ILogService logService)
        {
            _unitOfRepository = unitOfRepository;
            _unitOfCommand = unitOfCommand;
            _logService = logService;
        }

        public async Task<Result> Handle(ChooseAccessCommand command, CancellationToken cancellationToken)
        {
            var logger = _logService.GetLogger(command.AccountId);
            var accesses = _unitOfRepository.AccountRepository.GetAccesses(command.AccountId);

            var access = await GetValidAccess(accesses, logger);
            if (access is null) return Result.Fail(NoAccessAvailable.AllAccessNotWorking);

            if (accesses.Count == 1)
            {
                Value = access;
                return Result.Ok();
            }
            if (command.IgnoreSleepTime)
            {
                Value = access;
                return Result.Ok();
            }

            var minSleep = _unitOfRepository.AccountSettingRepository.GetByName(command.AccountId, AccountSettingEnums.SleepTimeMin);

            var timeValid = DateTime.Now.AddMinutes(-minSleep);
            if (access.LastUsed > timeValid) return Result.Fail(NoAccessAvailable.LackOfAccess);

            Value = access;
            return Result.Ok();
        }

        private async Task<AccessDto> GetValidAccess(List<AccessDto> accesses, ILogger logger)
        {
            foreach (var access in accesses)
            {
                logger.Information("Check connection {proxy}", access.Proxy);
                var valid = await _unitOfCommand.ValidateProxyCommand.Execute(access);
                if (!valid)
                {
                    logger.Warning("Connection {proxy} is not working", access.Proxy);
                    continue;
                }
                logger.Information("Connection {proxy} is working", access.Proxy);
                return access;
            }
            return null;
        }
    }
}