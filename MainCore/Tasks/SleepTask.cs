using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;
using Serilog;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class SleepTask : AccountTask
    {
        private readonly ITaskManager _taskManager;
        private readonly ILogService _logService;
        private readonly IChromeManager _chromeManager;
        private ILogger _logger;
        private readonly ICommandHandler<OpenBrowserCommand> _openBrowserCommand;

        public SleepTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ITaskManager taskManager, ILogService logService, IChromeManager chromeManager, ICommandHandler<OpenBrowserCommand> openBrowserCommand) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _taskManager = taskManager;
            _logService = logService;
            _chromeManager = chromeManager;
            _openBrowserCommand = openBrowserCommand;
        }

        protected override async Task<Result> Execute()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            await Task.Run(chromeBrowser.Close, CancellationToken.None);

            _logger = _logService.GetLogger(AccountId);

            Result result;
            result = await Sleep();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var accessResult = await GetAccess();
            if (accessResult.IsFailed) return Result.Fail(accessResult.Errors).WithError(TraceMessage.Error(TraceMessage.Line()));
            var access = accessResult.Value;

            result = await _openBrowserCommand.Handle(new(AccountId, access), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            await SetNextExecute();

            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var workTime = _unitOfRepository.AccountSettingRepository.GetByName(AccountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            ExecuteAt = DateTime.Now.AddMinutes(workTime);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            _name = "Sleep task";
        }

        private async Task<Result> Sleep()
        {
            var sleepTimeMinutes = _unitOfRepository.AccountSettingRepository.GetByName(AccountId, AccountSettingEnums.SleepTimeMin, AccountSettingEnums.SleepTimeMax);
            var sleepEnd = DateTime.Now.AddMinutes(sleepTimeMinutes);
            int lastMinute = 0;
            while (true)
            {
                if (CancellationToken.IsCancellationRequested) return Cancel.Error;
                var timeRemaining = sleepEnd - DateTime.Now;
                if (timeRemaining < TimeSpan.Zero) return Result.Ok();
                await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
                var currentMinute = (int)timeRemaining.TotalMinutes;
                if (lastMinute != currentMinute)
                {
                    _logger.Information("Chrome will reopen in {currentMinute} mins", currentMinute);
                    lastMinute = currentMinute;
                }
            }
        }

        public async Task<Result<AccessDto>> GetAccess()
        {
            var accesses = _unitOfRepository.AccountRepository.GetAccesses(AccountId);
            var access = await GetValidAccess(accesses);
            if (access is null) return NoAccessAvailable.AllAccessNotWorking;

            _unitOfRepository.AccountRepository.UpdateAccessLastUsed(access.Id);

            if (accesses.Count == 1)
            {
                return access;
            }

            var minSleep = _unitOfRepository.AccountSettingRepository.GetByName(AccountId, AccountSettingEnums.SleepTimeMin);
            var timeValid = DateTime.Now.AddMinutes(-minSleep);
            if (access.LastUsed > timeValid) return NoAccessAvailable.LackOfAccess;

            return access;
        }

        private async Task<AccessDto> GetValidAccess(List<AccessDto> accesses)
        {
            foreach (var access in accesses)
            {
                _logger.Information("Check connection {proxy}", access.Proxy);
                var result = await _unitOfCommand.ValidateProxyCommand.Handle(new(access), CancellationToken);
                if (result.IsFailed) return null;
                if (!_unitOfCommand.ValidateProxyCommand.Value)
                {
                    _logger.Warning("Connection {proxy} cannot connect to travian.com", access.Proxy);
                    continue;
                }
                _logger.Information("Connection {proxy} is working", access.Proxy);
                return access;
            }
            return null;
        }
    }
}