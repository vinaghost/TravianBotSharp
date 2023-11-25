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
        private readonly IChooseAccessCommand _chooseAccessCommand;
        private readonly ISleepBrowserCommand _sleepBrowserCommand;
        private readonly IOpenBrowserCommand _openBrowserCommand;
        private readonly IUnitOfRepository _unitOfRepository;

        public SleepCommandHandler(IChooseAccessCommand chooseAccessCommand, ISleepBrowserCommand sleepBrowserCommand, IOpenBrowserCommand openBrowserCommand, IUnitOfRepository unitOfRepository)
        {
            _chooseAccessCommand = chooseAccessCommand;
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

            result = await _chooseAccessCommand.Execute(accountId, false);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var access = _chooseAccessCommand.Value;
            result = _openBrowserCommand.Execute(accountId, access);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}