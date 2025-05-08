using MainCore.Notifications.Handlers.Refresh;

namespace MainCore.Notifications.Behaviors
{
    public sealed class AccountListUpdatedBehavior<TRequest, TResponse>
       : Behavior<TRequest, TResponse>
    {
        private readonly AccountListRefresh.Handler _accountListRefresh;

        public AccountListUpdatedBehavior(AccountListRefresh.Handler accountListRefresh)
        {
            _accountListRefresh = accountListRefresh;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);
            await _accountListRefresh.HandleAsync(new MainCore.Constraints.Notification(), cancellationToken);
            return response;
        }
    }
}