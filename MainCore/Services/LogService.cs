using Serilog;
using Serilog.Events;

namespace MainCore.Services
{
    [RegisterSingleton<ILogService, LogService>]
    public sealed class LogService : ILogService
    {
        public Dictionary<AccountId, ILogger> Loggers { get; } = [];
        private readonly LogSink _logSink;

        public LogService(LogSink logSink)
        {
            _logSink = logSink;
        }

        public void Shutdown()
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
