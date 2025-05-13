namespace MainCore.Commands.Behaviors
{
    public sealed class CommandLoggingBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
            where TResponse : Result
    {
        private readonly ILogger _logger;

        public CommandLoggingBehavior(ILogger logger)
        {
            _logger = logger;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            _logger.Information("Execute {Command}", request.GetType().FullName);
            var response = await Next(request, cancellationToken);
            if (response.IsFailed)
            {
                _logger.Warning("{error}", response.ToString());
            }
            return response;
        }
    }
}