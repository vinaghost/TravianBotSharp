using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpdateVillageTask : VillageTask
    {
        private readonly ITaskManager _taskManager;
        private readonly IVillageSettingRepository _villageSettingRepository;

        public UpdateVillageTask(IMediator mediator, IVillageRepository villageRepository, ITaskManager taskManager, IVillageSettingRepository villageSettingRepository) : base(mediator, villageRepository)
        {
            _taskManager = taskManager;
            _villageSettingRepository = villageSettingRepository;
        }

        protected override async Task<Result> Execute()
        {
            var url = _chromeBrowser.CurrentUrl;
            Result result;
            await _chromeBrowser.Refresh(CancellationToken);
            if (url.Contains("dorf1"))
            {
                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else if (url.Contains("dorf2"))
            {
                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await new ToDorfCommand().Execute(_chromeBrowser, 1, false, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                result = await new ToDorfCommand().Execute(_chromeBrowser, 1, false, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var seconds = _villageSettingRepository.GetByName(VillageId, VillageSettingEnums.AutoRefreshMin, VillageSettingEnums.AutoRefreshMax, 60);
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