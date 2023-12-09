using FluentResults;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.StartFarmlist;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.Features
{
    public class StartFarmListCommand : ByAccountIdBase, IRequest<Result>
    {
        public StartFarmListCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class StartFarmListCommandHandler : IRequestHandler<StartFarmListCommand, Result>
    {
        private readonly ICommandHandler<StartSingleFarmListCommand> _startSingleFarmListCommand;
        private readonly ICommandHandler<StartAllFarmListCommand> _startAllFarmListCommand;
        private readonly UnitOfRepository _unitOfRepository;
        private readonly UnitOfCommand _unitOfCommand;

        public StartFarmListCommandHandler(ICommandHandler<StartSingleFarmListCommand> startSingleFarmListCommand, ICommandHandler<StartAllFarmListCommand> startAllFarmListCommand, UnitOfRepository unitOfRepository, UnitOfCommand unitOfCommand)
        {
            _startSingleFarmListCommand = startSingleFarmListCommand;
            _startAllFarmListCommand = startAllFarmListCommand;
            _unitOfRepository = unitOfRepository;
            _unitOfCommand = unitOfCommand;
        }

        public async Task<Result> Handle(StartFarmListCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            Result result;
            var useStartAllButton = _unitOfRepository.AccountSettingRepository.GetBooleanByName(accountId, AccountSettingEnums.UseStartAllButton);
            if (useStartAllButton)
            {
                result = await _startAllFarmListCommand.Handle(new(accountId), cancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            else
            {
                var farmLists = _unitOfRepository.FarmRepository.GetActive(accountId);
                if (farmLists.Count == 0) return Result.Fail(new Skip("No farmlist is active"));

                foreach (var farmList in farmLists)
                {
                    result = await _startSingleFarmListCommand.Handle(new(accountId, farmList), cancellationToken);
                    if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

                    await _unitOfCommand.DelayClickCommand.Handle(new(accountId), cancellationToken);
                }
            }
            return Result.Ok();
        }
    }
}