using Microsoft.Extensions.Logging;

namespace MainCore.Tasks.Behaviors
{
    public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        : Behavior<TRequest, TResponse>
    {
        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            logger.LogInformation("LoggingBehavior.Enter");
            var response = await Next(request, cancellationToken);
            logger.LogInformation("LoggingBehavior.Exit");
            return response;
        }
    }
}