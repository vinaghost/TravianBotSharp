namespace MainCore.Behaviors
{
    public sealed class CommandLoggingBehavior<TRequest, TResponse>(ILogger logger)
        : Behavior<TRequest, TResponse>
        where TRequest : ICommand
    {
        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            LogCommand(request);
            var response = await Next(request, cancellationToken);
            return response;
        }

        private void LogCommand(TRequest request)
        {
            var name = request.Name;
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            logger.Information("Execute {Name}", name);
        }
    }
}