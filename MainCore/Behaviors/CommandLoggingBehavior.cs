namespace MainCore.Behaviors
{
    public sealed class CommandLoggingBehavior<TRequest, TResponse>(ILogger logger)
        : Behavior<TRequest, TResponse>
        where TRequest : ICommand
    {
        private static readonly string[] ExcludedCommandNames =
        [
            "Update",
            "Delay",
            "NextExecute"
        ];

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            LogCommand(request);
            var response = await Next(request, cancellationToken);
            return response;
        }

        private void LogCommand(TRequest request)
        {
            var name = request.GetType().FullName;
            if (string.IsNullOrEmpty(name) || ExcludedCommandNames.Any(name.Contains))
            {
                return;
            }

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
                logger.Information("Execute {Name}", name);
            }
            else
            {
                logger.Information("Execute {Name} {@Dict}", name, dict);
            }
        }
    }
}