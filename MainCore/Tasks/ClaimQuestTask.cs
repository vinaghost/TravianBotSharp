using MainCore.Commands.Features;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class ClaimQuestTask : VillageTask
    {
        public ClaimQuestTask(IChromeManager chromeManager, IMediator mediator, IVillageRepository villageRepository) : base(chromeManager, mediator, villageRepository)
        {
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            result = await _mediator.Send(new ClaimQuestCommand(AccountId), CancellationToken);
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