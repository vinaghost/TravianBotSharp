namespace MainCore.Commands.Features.StartFarmList
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
        private readonly IMediator _mediator;

        public ToFarmListPageCommandHandler(UnitOfRepository unitOfRepository, UnitOfCommand unitOfCommand, IMediator mediator)
        {
            _unitOfRepository = unitOfRepository;
            _unitOfCommand = unitOfCommand;
            _mediator = mediator;
        }

        public async Task<Result> Handle(ToFarmListPageCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            Result result;
            result = await _mediator.Send(new UpdateVillageListCommand(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var rallypointVillageId = _villageRepository.GetVillageHasRallypoint(accountId);
            if (rallypointVillageId == VillageId.Empty) return Skip.NoRallypoint;

            result = await _unitOfCommand.SwitchVillageCommand.Handle(new(accountId, rallypointVillageId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(ToDorfCommand.ToDorf2(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new UpdateBuildingCommand(accountId, rallypointVillageId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new ToBuildingCommand(chromeBrowser, 39), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _unitOfCommand.SwitchTabCommand.Handle(new(accountId, 4), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateFarmListCommand.Handle(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}