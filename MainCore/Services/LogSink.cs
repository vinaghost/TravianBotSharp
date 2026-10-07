using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

using Splat;

namespace MainCore.Services
{
    [RegisterSingleton<LogSink>]
    public sealed class LogSink : ILogEventSink
    {
        private Dictionary<AccountId, LinkedList<LogEvent>> Logs { get; } = [];

        private readonly RxQueue _rxQueue;

        public LogSink(RxQueue rxQueue)
        {
            _rxQueue = rxQueue;
        }

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
            var value = scalarValue.Value as string;
            var accountId = new AccountId(int.Parse(value!));

            var logs = GetLogs(accountId);
            logs.AddLast(logEvent);
            // keeps 200 message
            if (logs.Count > 200)
            {
                logs.RemoveFirst();
            }

            _rxQueue.Enqueue(new LogEmitted(accountId, logEvent));
        }
    }

    public static class LogSinkExtensions
    {
        public static LoggerConfiguration LogSink(
                  this LoggerSinkConfiguration loggerConfiguration)
        {
            return loggerConfiguration.Sink(Locator.Current.GetService<LogSink>()!);
        }
    }
}