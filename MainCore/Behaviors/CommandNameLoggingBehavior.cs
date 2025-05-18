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
            var name = request.GetType().FullName;
            if (!string.IsNullOrEmpty(name) && !name.Contains("Update") && !name.Contains("Delay"))
            {
                name = name
                    .Replace("MainCore.", "")
                    .Replace("+Command", "");

                var dict = request.GetType().GetProperties()
                    .Where(prop => !prop.Name.Equals("AccountId"))
                    .Where(prop => !prop.Name.Equals("VillageId"))
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(request)?.ToString() ?? "");

                if (dict.Count == 0)
                {
                    _logger.Information("Execute {name}", name);
                }
                else
                {
                    _logger.Information("Execute {name} {@dict}", name, dict);
                }
            }

            var response = await Next(request, cancellationToken);
            return response;
        }
    }
}