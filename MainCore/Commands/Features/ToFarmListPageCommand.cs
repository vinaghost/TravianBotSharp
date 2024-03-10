using FluentResults;
using MainCore.Common.Errors;
using MainCore.Common.Errors.Farming;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.Features
{
    public class ToFarmListPageCommand : ByAccountIdBase, IRequest<Result>
    {
        public ToFarmListPageCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class ToFarmListPageCommandHandler : IRequestHandler<ToFarmListPageCommand, Result>
    {
        private readonly UnitOfRepository _unitOfRepository;
        private readonly UnitOfCommand _unitOfCommand;

        public ToFarmListPageCommandHandler(UnitOfRepository unitOfRepository, UnitOfCommand unitOfCommand)
        {
            _unitOfRepository = unitOfRepository;
            _unitOfCommand = unitOfCommand;
        }

        public async Task<Result> Handle(ToFarmListPageCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            Result result;
            result = await _unitOfCommand.UpdateVillageListCommand.Handle(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var rallypointVillageId = _unitOfRepository.VillageRepository.GetVillageHasRallypoint(accountId);
            if (rallypointVillageId == VillageId.Empty) return Result.Fail(new NoRallypoint());

            result = await _unitOfCommand.ToDorfCommand.Handle(new(accountId, 2), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.SwitchVillageCommand.Handle(new(accountId, rallypointVillageId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(accountId, rallypointVillageId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.ToBuildingCommand.Handle(new(accountId, 39), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.SwitchTabCommand.Handle(new(accountId, 4), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateFarmListCommand.Handle(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}