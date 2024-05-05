using MainCore.Commands.Features.ClaimQuest;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class ClaimQuestTask : VillageTask
    {
        private readonly DelayClickCommand _delayClickCommand;

        public ClaimQuestTask(IVillageRepository villageRepository, DelayClickCommand delayClickCommand) : base(villageRepository)
        {
            _delayClickCommand = delayClickCommand;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            result = await new ToQuestPageCommand().Execute(_chromeBrowser, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await new ClaimQuestCommand().Execute(AccountId, _chromeBrowser, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _mediator.Publish(new StorageUpdated(AccountId, VillageId), CancellationToken);
            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = _villageRepository.GetVillageName(VillageId);
            _name = $"Claim quest in {village}";
        }
    }
}