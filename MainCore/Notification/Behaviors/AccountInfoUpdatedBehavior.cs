using MainCore.Constraints;

namespace MainCore.Notification.Behaviors
{
    public sealed class AccountInfoUpdatedBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
            where TRequest : IAccountConstraint
    {
        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);
            return response;
        }
    }
}