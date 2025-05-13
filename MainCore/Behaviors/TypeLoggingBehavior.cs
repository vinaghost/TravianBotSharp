namespace MainCore.Behaviors
{
    public sealed class TypeLoggingBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
    {
        private readonly ILogger _logger;

        public TypeLoggingBehavior(ILogger logger)
        {
            _logger = logger;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var name = request.GetType().FullName.Replace("MainCore.", "");
            _logger.Information("Execute {Command}", name);

            var response = await Next(request, cancellationToken);
            return response;
        }
    }
}