namespace MainCore.Behaviors
{
    public sealed class ErrorLoggingBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
            where TResponse : Result
    {
        private readonly ILogger _logger;

        public ErrorLoggingBehavior(ILogger logger)
        {
            _logger = logger;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);
            if (response.IsFailed)
            {
                var message = string.Join(Environment.NewLine, response.Reasons.Select(e => e.Message));
                _logger.Warning("{error}", message);
            }
            return response;
        }
    }
}