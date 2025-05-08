using MainCore.Constraints;
using MainCore.Notifications.Handlers.Trigger;

namespace MainCore.Notifications.Behaviors
{
    public sealed class CompleteImmediatelyBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
            where TRequest : IAccountVillageNotification
    {
        private readonly UpgradeBuildingTaskTrigger.Handler _upgradeBuildingTaskTrigger;

        public CompleteImmediatelyBehavior(UpgradeBuildingTaskTrigger.Handler upgradeBuildingTaskTrigger)
        {
            _upgradeBuildingTaskTrigger = upgradeBuildingTaskTrigger;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);
            await _upgradeBuildingTaskTrigger.HandleAsync(request, cancellationToken);
            return response;
        }
    }
}