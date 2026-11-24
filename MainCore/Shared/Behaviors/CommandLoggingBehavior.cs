namespace MainCore.Shared.Behaviors
{
    public sealed class CommandLoggingBehavior<TRequest, TResponse>(ILogger logger)
        : Behavior<TRequest, TResponse>
        where TRequest : ICommand
        where TResponse : Result
    {
        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return (TResponse)Cancel.Error;
            }
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