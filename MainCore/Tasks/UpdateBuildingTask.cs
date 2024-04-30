using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpdateBuildingTask : VillageTask
    {
        private readonly IChromeManager _chromeManager;

        public UpdateBuildingTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, IChromeManager chromeManager) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _chromeManager = chromeManager;
        }

        protected override async Task<Result> Execute()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var url = chromeBrowser.CurrentUrl;
            Result result;
            if (url.Contains("dorf1"))
            {
                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _mediator.Send(ToDorfCommand.ToDorf2(AccountId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else if (url.Contains("dorf2"))
            {
                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _mediator.Send(ToDorfCommand.ToDorf1(AccountId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                result = await _mediator.Send(ToDorfCommand.ToDorf2(AccountId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _mediator.Send(ToDorfCommand.ToDorf1(AccountId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Update all buildings in {village}";
        }
    }
}