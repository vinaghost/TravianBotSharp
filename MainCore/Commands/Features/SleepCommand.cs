using FluentResults;
using MainCore.Commands.Base;
using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.Features
{
    public class SleepCommand : ByAccountIdBase, IRequest<Result>
    {
        public SleepCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class SleepCommandHandler : IRequestHandler<SleepCommand, Result>
    {
        private readonly ICommandHandler<ChooseAccessCommand, AccessDto> _chooseAccessCommand;
        private readonly ICommandHandler<SleepBrowserCommand> _sleepBrowserCommand;
        private readonly ICommandHandler<OpenBrowserCommand> _openBrowserCommand;
        private readonly IUnitOfRepository _unitOfRepository;

        public SleepCommandHandler(ICommandHandler<ChooseAccessCommand, AccessDto> chooseAccessCommand, ICommandHandler<SleepBrowserCommand> sleepBrowserCommand, ICommandHandler<OpenBrowserCommand> openBrowserCommand, IUnitOfRepository unitOfRepository)
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
            result = await _sleepBrowserCommand.Handle(new(accountId, TimeSpan.FromMinutes(sleepTimeMinutes)), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _chooseAccessCommand.Handle(new ChooseAccessCommand(accountId, false), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var access = _chooseAccessCommand.Value;
            result = await _openBrowserCommand.Handle(new(accountId, access), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}