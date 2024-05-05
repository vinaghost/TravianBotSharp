using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpdateVillageTask : VillageTask
    {
        private readonly ITaskManager _taskManager;

        public UpdateVillageTask(IMediator mediator, IVillageRepository villageRepository, ITaskManager taskManager) : base(mediator, villageRepository)
        {
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute()
        {
            var url = _chromeBrowser.CurrentUrl;
            Result result;
            await _chromeBrowser.Refresh(CancellationToken);

            var updateBuildingCommand = new UpdateBuildingCommand();

            if (url.Contains("dorf1"))
            {
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
                result = await new ToDorfCommand().Execute(_chromeBrowser, 1, false, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await updateBuildingCommand.Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);
            }

            await new UpdateStorageCommand().Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);

            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var seconds = new GetVillageSetting().ByName(VillageId, VillageSettingEnums.AutoRefreshMin, VillageSettingEnums.AutoRefreshMax, 60);
            ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            var village = _villageRepository.GetVillageName(VillageId);
            _name = $"Update village in {village}";
        }
    }
}