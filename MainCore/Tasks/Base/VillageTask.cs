namespace MainCore.Tasks.Base
{
    public abstract class VillageTask : AccountTask
    {
        protected readonly IVillageRepository _villageRepository;

        protected VillageTask(IChromeManager chromeManager, IMediator mediator, IVillageRepository villageRepository) : base(chromeManager, mediator)
        {
            _villageRepository = villageRepository;
        }

        public VillageId VillageId { get; protected set; }

        public void Setup(AccountId accountId, VillageId villageId, CancellationToken cancellationToken = default)
        {
            AccountId = accountId;
            VillageId = villageId;
            CancellationToken = cancellationToken;
        }

        protected override async Task<Result> PreExecute()
        {
            Result result;
            result = await base.PreExecute();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new SwitchVillageCommand(_chromeBrowser, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override async Task<Result> PostExecute()
        {
            Result result;

            result = await base.PostExecute();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(ToDorfCommand.ToDorf(AccountId));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new CheckQuestCommand(AccountId, VillageId, _chromeBrowser));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}