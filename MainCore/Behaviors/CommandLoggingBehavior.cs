using MainCore.Infrastructure;

namespace MainCore.Behaviors
{
    public sealed class CommandLoggingBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
        where TRequest : ICommand
    {
        private readonly ILogger _logger;
        private readonly CommandLoggingConfig _config;

        public CommandLoggingBehavior(ILogger logger, CommandLoggingConfig config)
        {
            _logger = logger;
            _config = config;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var commandFullName = request.GetType().FullName;
            if (!string.IsNullOrEmpty(commandFullName) && _config.ShouldLog(commandFullName))
            {
                var logLevel = _config.GetLogLevel(commandFullName);
                
                var simpleName = commandFullName
                    .Replace("MainCore.", "")
                    .Replace("+Command", "");

                var dict = request.GetType().GetProperties()
                    .Where(prop => !prop.Name.Equals("AccountId"))
                    .Where(prop => !prop.Name.Equals("VillageId"))
                    .ToDictionary(prop => prop.Name, prop =>
                    {
                        var value = prop.GetValue(request);
                        return value is long[] array ? string.Join(",", array) : value?.ToString() ?? "";
                    });

                if (dict.Count == 0)
                {
                    _logger.Write(logLevel, "Execute {Name}", simpleName);
                }
                else
                {
                    _logger.Write(logLevel, "Execute {Name} {@Dict}", simpleName, dict);
                }
            }

            var response = await Next(request, cancellationToken);
            return response;
        }
    }
}