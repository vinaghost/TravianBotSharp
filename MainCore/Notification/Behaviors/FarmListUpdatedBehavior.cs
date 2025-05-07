using MainCore.Constraints;
using MainCore.Notification.Handlers.Refresh;

namespace MainCore.Notification.Behaviors
{
    public sealed class FarmListUpdatedBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
        where TRequest : IAccountConstraint
    {
        private readonly FarmListRefresh.Handler _farmListRefresh;

        public FarmListUpdatedBehavior(FarmListRefresh.Handler farmListRefresh)
        {
            _farmListRefresh = farmListRefresh;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);
            await _farmListRefresh.HandleAsync(request, cancellationToken);
            return response;
        }
    }
}