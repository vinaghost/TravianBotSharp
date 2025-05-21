using MainCore.Constraints;

namespace MainCore.Behaviors
{
    public sealed class NextExecuteLoggingBehaviors<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
        where TRequest : ITask
    {
        private readonly ILogger _logger;

        public NextExecuteLoggingBehaviors(ILogger logger)
        {
            _logger = logger;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);
            _logger.Information("Schedule next run at {Time}", request.ExecuteAt.ToString("yyyy-MM-dd HH:mm:ss"));
            return response;
        }
    }
}