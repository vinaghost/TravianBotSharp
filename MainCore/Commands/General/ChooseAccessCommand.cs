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
using Polly;
using Polly.Retry;
using RestSharp;
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

        private readonly UnitOfRepository _unitOfRepository;
        private readonly ILogService _logService;
        private readonly IRestClientManager _restClientManager;

        private static readonly AsyncRetryPolicy<bool> _retryPolicy = Policy<bool>
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: times => TimeSpan.FromSeconds(10 * times));

        public ChooseAccessCommandHandler(UnitOfRepository unitOfRepository, ILogService logService, IRestClientManager restClientManager)
        {
            _unitOfRepository = unitOfRepository;
            _logService = logService;
            _restClientManager = restClientManager;
        }

        public async Task<Result> Handle(ChooseAccessCommand command, CancellationToken cancellationToken)
        {
            var logger = _logService.GetLogger(command.AccountId);
            var accesses = _unitOfRepository.AccountRepository.GetAccesses(command.AccountId);

            var access = await GetValidAccess(accesses, logger, cancellationToken);
            if (access is null) return NoAccessAvailable.AllAccessNotWorking;

            _unitOfRepository.AccountRepository.UpdateAccessLastUsed(access.Id);

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
            if (access.LastUsed > timeValid) return NoAccessAvailable.LackOfAccess;

            Value = access;
            return Result.Ok();
        }

        private async Task<AccessDto> GetValidAccess(List<AccessDto> accesses, ILogger logger, CancellationToken cancellationToken)
        {
            foreach (var access in accesses)
            {
                logger.Information("Check connection {proxy}", access.Proxy);

                var poliResult = await _retryPolicy
                    .ExecuteAndCaptureAsync(() => Validate(access));

                if (!poliResult.Result)
                {
                    logger.Warning("Connection {proxy} cannot connect to travian.com", access.Proxy);
                    continue;
                }
                logger.Information("Connection {proxy} is working", access.Proxy);
                return access;
            }
            return null;
        }

        private async Task<bool> Validate(AccessDto access)
        {
            var request = new RestRequest
            {
                Method = Method.Get,
            };
            var client = _restClientManager.Get(access);
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful) throw new Exception("Proxy failed");
            return true;
        }
    }
}