using MainCore.Commands.Base;

namespace MainCore.Commands.Behaviors
{
    public sealed class CommandLoggingBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
            where TRequest : IAccountCommand
            where TResponse : Result
    {
        private readonly ILogService _logService;

        public CommandLoggingBehavior(ILogService logService)
        {
            _logService = logService;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var logger = _logService.GetLogger(request.AccountId);
            logger.Information("Execute {Command}", request.GetType().Name);
            var response = await Next(request, cancellationToken);
            if (response.IsFailed)
            {
                logger.Warning("{error}", response.ToString());
            }
            return response;
        }
    }
}