using FluentResults;
using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.Special
{
    public class SleepCommand : ByAccountIdBase, IRequest<Result>
    {
        public SleepCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class SleepCommandHandler : IRequestHandler<SleepCommand, Result>
    {
        private readonly ChooseAccessCommandHandler _chooseAccessCommandHandler;
        private readonly SleepBrowserCommandHandler _sleepBrowserCommand;
        private readonly OpenBrowserCommandHandler _openBrowserCommand;
        private readonly IUnitOfRepository _unitOfRepository;

        public SleepCommandHandler(ChooseAccessCommandHandler chooseAccessCommandHandler, SleepBrowserCommandHandler sleepBrowserCommand, OpenBrowserCommandHandler openBrowserCommand, IUnitOfRepository unitOfRepository)
        {
            _chooseAccessCommandHandler = chooseAccessCommandHandler;
            _sleepBrowserCommand = sleepBrowserCommand;
            _openBrowserCommand = openBrowserCommand;
            _unitOfRepository = unitOfRepository;
        }

        public async Task<Result> Handle(SleepCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var sleepTimeMinutes = _unitOfRepository.AccountSettingRepository.GetByName(accountId, AccountSettingEnums.SleepTimeMin, AccountSettingEnums.SleepTimeMax);
            Result result;
            result = await _sleepBrowserCommand.Execute(accountId, TimeSpan.FromMinutes(sleepTimeMinutes), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _chooseAccessCommandHandler.Handle(new ChooseAccessCommand(accountId, false), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var access = _chooseAccessCommandHandler.Value;
            result = await _openBrowserCommand.Execute(accountId, access, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}