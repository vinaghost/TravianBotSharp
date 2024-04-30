using MainCore.Commands.Misc;

namespace MainCore.Tasks.Base
{
    public abstract class VillageTask : AccountTask
    {
        protected VillageTask(IChromeManager chromeManager, UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator) : base(chromeManager,unitOfCommand, unitOfRepository, mediator)
        {
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

            result = await _mediator.Send(new SwitchVillageCommand(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override async Task<Result> PostExecute()
        {
            Result result;
            result = await _mediator.Send(new CheckQuestCommand(AccountId, VillageId));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}