namespace MainCore.Behaviors
{
    public sealed class CommandLoggingBehavior<TRequest, TResponse>
        : Behavior<TRequest, TResponse>
        where TRequest : ICommand
    {
        private readonly ILogger _logger;

        public CommandLoggingBehavior(ILogger logger)
        {
            _logger = logger;
        }

        private static readonly string[] ExcludedCommandNames = new[]
        {
            "Update",
            "Delay",
            "NextExecute"
        };

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var name = request.GetType().FullName;
            if (!string.IsNullOrEmpty(name) && !ExcludedCommandNames.Any(name.Contains))
            {
                name = name
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
                    _logger.Information("Execute {Name}", name);
                }
                else
                {
                    _logger.Information("Execute {Name} {@Dict}", name, dict);
                }
            }

            var response = await Next(request, cancellationToken);
            return response;
        }
    }
}
