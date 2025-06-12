using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace MainCore.Services
{
    [RegisterSingleton<LogSink>]
    public sealed class LogSink : ILogEventSink
    {
        private Dictionary<AccountId, LinkedList<LogEvent>> Logs { get; } = [];

        public event Action<AccountId, LogEvent> LogEmitted = delegate { };

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
            logs.AddFirst(logEvent);
            // keeps 200 message
            if (logs.Count > 200)
            {
                logs.RemoveLast();
            }

            LogEmitted.Invoke(accountId, logEvent);

            try
            {
                var scopeFactory = Locator.Current.GetService<ICustomServiceScopeFactory>();
                var telegram = Locator.Current.GetService<ITelegramService>();
                if (scopeFactory is not null && telegram is not null)
                {
                    using var scope = scopeFactory.CreateScope(accountId);
                    var settingService = scope.ServiceProvider.GetRequiredService<ISettingService>();
                    var enable = settingService.BooleanByName(accountId, AccountSettingEnums.EnableTelegramMessage);
                    if (enable)
                    {
                        _ = telegram.SendText(logEvent.RenderMessage(), accountId);
                    }
                }
            }
            catch (Exception)
            {
                // ignore telegram errors
            }
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