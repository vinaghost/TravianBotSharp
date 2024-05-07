namespace MainCore.Tasks.Base
{
    public abstract class VillageTask : AccountTask
    {
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

            result = await new SwitchVillageCommand().Execute(_chromeBrowser, VillageId, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await new UpdateStorageCommand().Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);

            return Result.Ok();
        }

        protected override async Task<Result> PostExecute()
        {
            Result result;

            result = await base.PostExecute();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await new ToDorfCommand().Execute(_chromeBrowser, 0, false, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await new UpdateBuildingCommand().Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);
            await new UpdateStorageCommand().Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);
            await new CheckQuestCommand().Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);
            return Result.Ok();
        }
    }
}