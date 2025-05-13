using MainCore.Constraints;

namespace MainCore.Behaviors
{
    public sealed class CommandNameLoggingBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
        where TRequest : ICommand
    {
        private readonly ILogger _logger;

        public CommandNameLoggingBehavior(ILogger logger)
        {
            _logger = logger;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var name = request.GetType().FullName
                .Replace("MainCore.", "")
                .Replace("+Command", "");

            _logger.Information("Execute {Command}", name);

            var response = await Next(request, cancellationToken);
            return response;
        }
    }
}