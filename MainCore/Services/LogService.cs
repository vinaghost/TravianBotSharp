using Serilog;
using Serilog.Events;

namespace MainCore.Services
{
    [RegisterSingleton<LogService>]
    public sealed class LogService(LogSink logSink)
    {
        public Dictionary<AccountId, ILogger> Loggers { get; } = [];
        private readonly LogSink _logSink = logSink;

        public static void Shutdown()
        {
            Log.CloseAndFlush();
        }

        public LinkedList<LogEvent> GetLog(AccountId accountId)
        {
            var logs = _logSink.GetLogs(accountId);
            return logs;
        }
    }
}