using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpdateBuildingTask : VillageTask
    {
        protected override async Task<Result> Execute()
        {
            var url = _chromeBrowser.CurrentUrl;
            Result result;
            var updateBuildingCommand = new UpdateBuildingCommand();

            if (url.Contains("dorf1"))
            {
                await updateBuildingCommand.Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);

                result = await new ToDorfCommand().Execute(_chromeBrowser, 2, false, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await updateBuildingCommand.Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);
            }
            else if (url.Contains("dorf2"))
            {
                await updateBuildingCommand.Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);

                result = await new ToDorfCommand().Execute(_chromeBrowser, 1, false, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await updateBuildingCommand.Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);
            }
            else
            {
                result = await new ToDorfCommand().Execute(_chromeBrowser, 2, false, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await updateBuildingCommand.Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);

                result = await new ToDorfCommand().Execute(_chromeBrowser, 1, false, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await updateBuildingCommand.Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);
            }

            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = new GetVillageName().Execute(VillageId);
            _name = $"Update all buildings in {village}";
        }
    }
}