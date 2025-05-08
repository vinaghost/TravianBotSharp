using MainCore.Constraints;
using MainCore.Notifications.Handlers.Refresh;
using MainCore.Notifications.Handlers.Trigger;

namespace MainCore.Notifications.Behaviors
{
    public sealed class VillageSettingUpdatedBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
            where TRequest : IAccountVillageConstraint
    {
        private readonly ChangeWallTrigger.Handler _changeWallTrigger;
        private readonly ClaimQuestTaskTrigger.Handler _claimQuestTaskTrigger;
        private readonly CompleteImmediatelyTaskTrigger.Handler _completeImmediatelyTaskTrigger;
        private readonly NpcTaskTrigger.Handler _npcTaskTrigger;
        private readonly RefreshVillageTaskTrigger.Handler _refreshVillageTaskTrigger;
        private readonly TrainTroopTaskTrigger.Handler _trainTroopTaskTrigger;
        private readonly VillageSettingRefresh.Handler _villageSettingRefresh;

        public VillageSettingUpdatedBehavior(ChangeWallTrigger.Handler changeWallTrigger, ClaimQuestTaskTrigger.Handler claimQuestTaskTrigger, CompleteImmediatelyTaskTrigger.Handler completeImmediatelyTaskTrigger, NpcTaskTrigger.Handler npcTaskTrigger, RefreshVillageTaskTrigger.Handler refreshVillageTaskTrigger, TrainTroopTaskTrigger.Handler trainTroopTaskTrigger, VillageSettingRefresh.Handler villageSettingRefresh)
        {
            _changeWallTrigger = changeWallTrigger;
            _claimQuestTaskTrigger = claimQuestTaskTrigger;
            _completeImmediatelyTaskTrigger = completeImmediatelyTaskTrigger;
            _npcTaskTrigger = npcTaskTrigger;
            _refreshVillageTaskTrigger = refreshVillageTaskTrigger;
            _trainTroopTaskTrigger = trainTroopTaskTrigger;
            _villageSettingRefresh = villageSettingRefresh;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);

            await _changeWallTrigger.HandleAsync(request, cancellationToken);
            await _claimQuestTaskTrigger.HandleAsync(request, cancellationToken);
            await _completeImmediatelyTaskTrigger.HandleAsync(request, cancellationToken);
            await _npcTaskTrigger.HandleAsync(request, cancellationToken);
            await _refreshVillageTaskTrigger.HandleAsync(request, cancellationToken);
            await _trainTroopTaskTrigger.HandleAsync(request, cancellationToken);
            await _villageSettingRefresh.HandleAsync(request, cancellationToken);
            return response;
        }
    }
}