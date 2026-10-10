namespace MainCore.Behaviors
{
    public sealed class TaskNameLoggingBehavior<TRequest, TResponse>(ILogger logger)
        : Behavior<TRequest, TResponse>
        where TRequest : ITask
        where TResponse : Result
    {
        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            logger.Information("Task {TaskName} is started", request.Description);

            var response = await Next(request, cancellationToken);

            if (response.IsFailed)
            {
                logger.Warning("Task {TaskName} failed", request.Description);
                var message = string.Join(Environment.NewLine, response.Reasons.Select(e => e.Message));
                if (!string.IsNullOrEmpty(message))
                {
                    logger.Warning("{Message}", message);
                }
            }
            else
            {
                logger.Information("Task {TaskName} is finished", request.Description);
            }
            return response;
        }
    }
}