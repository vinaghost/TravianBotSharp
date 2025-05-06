namespace MainCore.Commands.Behaviors
{
    public sealed class CommandLoggingBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
            where TResponse : Result
    {
        private readonly AppDbContext _context;
        private readonly ILogger _logger;

        public CommandLoggingBehavior(ILogger logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            _logger.Information("Execute {Command}", request.GetType().Name);
            var response = await Next(request, cancellationToken);
            if (response.IsFailed)
            {
                _logger.Warning("{error}", response.ToString());
            }
            return response;
        }
    }
}