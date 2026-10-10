namespace MainCore.Behaviors
{
    public sealed class TaskNameLoggingBehavior<TRequest, TResponse>(ILogger logger)
        : Behavior<TRequest, TResponse>
        where TRequest : ITask
    {
        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            logger.Information("Task {TaskName} is started", request.Description);

            var response = await Next(request, cancellationToken);

            logger.Information("Task {TaskName} is finished", request.Description);
            return response;
        }
    }
}