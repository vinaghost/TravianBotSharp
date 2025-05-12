using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace MainCore.Services
{
    [RegisterSingleton<ILogEventSink, LogSink>]
    public sealed class LogSink : ILogEventSink
    {
        [RegisterServices]
        public static void Register(IServiceCollection services)
        {
            services
                .AddSingleton(x => x.GetService<ILogEventSink>() as LogSink);
        }

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

    public static class LogSinkExtensions
    {
        public static LoggerConfiguration LogSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(Locator.Current.GetService<ILogEventSink>());
        }
    }
}