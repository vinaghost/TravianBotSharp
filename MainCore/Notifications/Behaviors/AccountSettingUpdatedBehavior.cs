using MainCore.Constraints;
using MainCore.Notifications.Handlers.Refresh;
using MainCore.Notifications.Handlers.Trigger;

namespace MainCore.Notifications.Behaviors
{
    public sealed class AccountSettingUpdatedBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
            where TRequest : IAccountConstraint
    {
        private readonly StartAdventureTaskTrigger.Handler _startAdventureTaskTrigger;
        private readonly AccountSettingRefresh.Handler _accountSettingRefresh;

        public AccountSettingUpdatedBehavior(StartAdventureTaskTrigger.Handler startAdventureTaskTrigger, AccountSettingRefresh.Handler accountSettingRefresh)
        {
            _startAdventureTaskTrigger = startAdventureTaskTrigger;
            _accountSettingRefresh = accountSettingRefresh;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);
            await _startAdventureTaskTrigger.HandleAsync(request, cancellationToken);
            await _accountSettingRefresh.HandleAsync(request, cancellationToken);
            return response;
        }
    }
}