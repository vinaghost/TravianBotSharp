namespace MainCore.Behaviors
{
    public sealed class ErrorLoggingBehavior<TRequest, TResponse>(ILogger logger)
        : Behavior<TRequest, TResponse>
        where TRequest : ICommand
        where TResponse : IResultBase
    {
        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var response = await Next(request, cancellationToken);

            if (response.IsFailed)
            {
                var message = string.Join(Environment.NewLine, response.Reasons.Select(e => e.Message));
                if (!string.IsNullOrEmpty(message))
                {
                    logger.Warning("{Message}", message);
                }
            }

            return response;
        }
    }
}