using Serilog.Context;

namespace MainCore.Behaviors
{
    public sealed class AccountDataLoggingBehavior<TRequest, TResponse>
       : Behavior<TRequest, TResponse>
           where TRequest : IAccountConstraint
    {
        private readonly IDataService _dataService;

        public AccountDataLoggingBehavior(IDataService dataService)
        {
            _dataService = dataService;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            if (_dataService.IsLoggerConfigured) return await Next(request, cancellationToken);
            if (request.AccountId != _dataService.AccountId) return await Next(request, cancellationToken);

            using (LogContext.PushProperty("Account", _dataService.AccountData))
            using (LogContext.PushProperty("AccountId", _dataService.AccountId))
            {
                _dataService.IsLoggerConfigured = true;
                var response = await Next(request, cancellationToken);
                _dataService.IsLoggerConfigured = false;
                return response;
            }
        }
    }
}
