using MainCore.Constraints;
using MainCore.Notification.Handlers.Trigger;

namespace MainCore.Notification.Behaviors
{
    public sealed class StorageUpdatedBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
        where TRequest : IVillageConstraint
    {
        private readonly NpcTaskTrigger.Handler _npcTaskTrigger;

        public StorageUpdatedBehavior(NpcTaskTrigger.Handler npcTaskTrigger)
        {
            _npcTaskTrigger = npcTaskTrigger;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);
            await _npcTaskTrigger.HandleAsync(request, cancellationToken);
            return response;
        }
    }
}