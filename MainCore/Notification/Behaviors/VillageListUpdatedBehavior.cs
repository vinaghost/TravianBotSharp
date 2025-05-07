using MainCore.Constraints;
using MainCore.Notification.Handlers.Refresh;
using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Behaviors
{
    public sealed class VillageListUpdatedBehavior<TRequest, TResponse>
       : Behavior<TRequest, TResponse>
           where TRequest : IAccountConstraint
    {
        private readonly BuildingUpdateTaskTrigger.Handler _buildingUpdateTaskTrigger;
        private readonly VillageListRefresh.Handler _villageListRefresh;

        public VillageListUpdatedBehavior(BuildingUpdateTaskTrigger.Handler buildingUpdateTaskTrigger, VillageListRefresh.Handler villageListRefresh)
        {
            _buildingUpdateTaskTrigger = buildingUpdateTaskTrigger;
            _villageListRefresh = villageListRefresh;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);
            await _buildingUpdateTaskTrigger.HandleAsync(request, cancellationToken);
            await _villageListRefresh.HandleAsync(request, cancellationToken);
            return response;
        }
    }
}