﻿using MainCore.Constraints;
using Serilog.Context;

namespace MainCore.Behaviors
{
    public sealed class AccountDataLoggingBehavior<TRequest, TResponse>
       : Behavior<TRequest, TResponse>
           where TRequest : IAccountConstraint
    {
        private readonly ILogger _logger;
        private readonly IDataService _dataService;

        public AccountDataLoggingBehavior(ILogger logger, IDataService dataService)
        {
            _logger = logger;
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