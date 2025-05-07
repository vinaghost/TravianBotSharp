using MainCore.Constraints;
using MainCore.Notification.Handlers.Refresh;
using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Behaviors
{
    public sealed class BuildingUpdatedBehavior<TRequest, TResponse>
       : Behavior<TRequest, TResponse>
           where TRequest : IVillageConstraint
    {
        private readonly BuildingListRefresh.Handler _buildingListRefresh;
        private readonly CompleteImmediatelyTaskTrigger.Handler _completeImmediatelyTaskTrigger;
        private readonly QueueRefresh.Handler _queueRefresh;

        public BuildingUpdatedBehavior(BuildingListRefresh.Handler buildingListRefresh, CompleteImmediatelyTaskTrigger.Handler completeImmediatelyTaskTrigger, QueueRefresh.Handler queueRefresh)
        {
            _buildingListRefresh = buildingListRefresh;
            _completeImmediatelyTaskTrigger = completeImmediatelyTaskTrigger;
            _queueRefresh = queueRefresh;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);
            await _buildingListRefresh.HandleAsync(request, cancellationToken);
            await _completeImmediatelyTaskTrigger.HandleAsync(request, cancellationToken);
            await _queueRefresh.HandleAsync(request, cancellationToken);
            return response;
        }
    }
}