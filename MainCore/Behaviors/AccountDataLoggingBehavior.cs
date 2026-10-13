using Serilog.Context;

namespace MainCore.Behaviors
{
    public sealed class AccountDataLoggingBehavior<TRequest, TResponse>(DataService dataService)
       : Behavior<TRequest, TResponse>
           where TRequest : IAccountConstraint
    {
        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            if (dataService.IsLoggerConfigured) return await Next(request, cancellationToken);
            if (request.AccountId != dataService.AccountId) return await Next(request, cancellationToken);

            using (LogContext.PushProperty("Account", dataService.AccountData))
            using (LogContext.PushProperty("AccountId", dataService.AccountId))
            {
                dataService.IsLoggerConfigured = true;
                var response = await Next(request, cancellationToken);
                dataService.IsLoggerConfigured = false;
                return response;
            }
        }
    }
}