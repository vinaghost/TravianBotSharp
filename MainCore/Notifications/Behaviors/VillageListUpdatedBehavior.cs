using MainCore.Constraints;
using MainCore.Notifications.Trigger;

namespace MainCore.Notifications.Behaviors
{
    public sealed class VillageListUpdatedBehavior<TRequest, TResponse>
       : Behavior<TRequest, TResponse>
           where TRequest : IAccountConstraint
    {
        private readonly UpdateBuildingTaskTrigger.Handler _buildingUpdateTaskTrigger;

        public VillageListUpdatedBehavior(UpdateBuildingTaskTrigger.Handler buildingUpdateTaskTrigger)
        {
            _buildingUpdateTaskTrigger = buildingUpdateTaskTrigger;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);
            await _buildingUpdateTaskTrigger.HandleAsync(request, cancellationToken);
            return response;
        }
    }
}