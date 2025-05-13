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
                _logger.Warning("{error}", response.ToString());
            }
            return response;
        }
    }
}