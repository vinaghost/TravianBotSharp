using MainCore.Constraints;
using Serilog.Context;

namespace MainCore.Commands.Behaviors
{
    public sealed class AccountDataLoggingBehavior<TRequest, TResponse>
       : Behavior<TRequest, TResponse>
           where TRequest : IAccountConstraint
    {
        private readonly AppDbContext _context;
        private readonly ILogger _logger;
        private readonly IDataService _dataService;

        public AccountDataLoggingBehavior(ILogger logger, AppDbContext context, IDataService dataService)
        {
            _logger = logger;
            _context = context;
            _dataService = dataService;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            if (_dataService.IsLoggerConfigured) return await Next(request, cancellationToken);

            var accountId = request.AccountId;

            var account = _context.Accounts
                .Where(x => x.Id == accountId.Value)
                .Select(x => new
                {
                    x.Username,
                    x.Server,
                })
                .First();

            var uri = new Uri(account.Server);

            using (LogContext.PushProperty("Account", $"{account.Username}_{uri.Host}"))
            using (LogContext.PushProperty("AccountId", accountId))
            {
                _dataService.IsLoggerConfigured = true;
                return await Next(request, cancellationToken);
            }
        }
    }
}