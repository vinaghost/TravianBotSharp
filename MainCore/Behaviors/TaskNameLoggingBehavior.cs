namespace MainCore.Behaviors
{
    public sealed class TaskNameLoggingBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
        where TRequest : ITask
    {
        private readonly ILogger _logger;

        public TaskNameLoggingBehavior(ILogger logger)
        {
            _logger = logger;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            _logger.Information("Task {TaskName} is started", request.Description);

            var response = await Next(request, cancellationToken);

            _logger.Information("Task {TaskName} is finished", request.Description);
            return response;
        }
    }
}
