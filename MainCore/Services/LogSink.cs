using Serilog.Core;
using Serilog.Events;

namespace MainCore.Services
{
    [RegisterSingleton(Registration = RegistrationStrategy.ImplementedInterfaces)]
    public sealed class LogSink : ILogEventSink
    {
        private Dictionary<AccountId, LinkedList<LogEvent>> Logs { get; } = [];

        public event Action<AccountId, LogEvent> LogEmitted;

        public LinkedList<LogEvent> GetLogs(AccountId accountId)
        {
            var logs = Logs.GetValueOrDefault(accountId);
            if (logs is null)
            {
                logs = new LinkedList<LogEvent>();
                Logs.Add(accountId, logs);
            }
            return logs;
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent.Level < LogEventLevel.Information) return;
            var logEventPropertyValue = logEvent.Properties.GetValueOrDefault("AccountId");
            if (logEventPropertyValue is null) return;
            if (logEventPropertyValue is not ScalarValue scalarValue) return;
            var accountId = new AccountId(int.Parse(scalarValue.Value as string));

            var logs = GetLogs(accountId);
            logs.AddFirst(logEvent);
            // keeps 200 message
            if (logs.Count > 200)
            {
                logs.RemoveLast();
            }

            LogEmitted?.Invoke(accountId, logEvent);
        }
    }
}