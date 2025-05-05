using MainCore.Queries.Base;
using Serilog.Templates;
using System.Text;

namespace MainCore.Commands.UI.DebugViewModel
{
    [Handler]
    public static partial class GetLogQuery
    {
        public sealed record Query(AccountId AccountId) : IQuery;
        private static readonly ExpressionTemplate _template = new("{@t:HH:mm:ss} [{@l:u3}] {@m}\n{@x}");

        private static async ValueTask<string> HandleAsync(
            Query query,
            LogSink logSink,
            CancellationToken token
        )
        {
            await Task.CompletedTask;
            var logs = logSink.GetLogs(query.AccountId);
            using var sw = new StringWriter(new StringBuilder());

            foreach (var log in logs)
            {
                _template.Format(log, sw);
            }
            return sw.ToString();
        }
    }
}