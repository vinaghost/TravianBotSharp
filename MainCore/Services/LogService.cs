using Serilog;
using Serilog.Events;
using System.Reflection;

namespace MainCore.Services
{
    [RegisterSingleton<ILogService, LogService>]
    public sealed class LogService : ILogService
    {
        private readonly Dictionary<AccountId, Serilog.ILogger> _loggers = [];

        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly LogSink _logSink;

        public LogService(IDbContextFactory<AppDbContext> contextFactory, LogSink logSink)
        {
            _contextFactory = contextFactory;
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

        public Serilog.ILogger GetLogger(AccountId accountId)
        {
            if (_loggers.ContainsKey(accountId)) return _loggers[accountId];

            using var context = _contextFactory.CreateDbContext();
            var account = context.Accounts
                .Where(x => x.Id == accountId.Value)
                .First();

            var uri = new Uri(account.Server);
            var logger = Log.ForContext("Account", $"{account.Username}_{uri.Host}")
                        .ForContext("AccountId", accountId);
            _loggers.Add(accountId, logger);
            logger.Information("===============> Current version: {Version} <===============", GetVersion());

            return logger;
        }

        private static string GetVersion()
        {
            var versionAssembly = Assembly.GetExecutingAssembly().GetName().Version;
            var version = new Version(versionAssembly.Major, versionAssembly.Minor, versionAssembly.Build);
            return $"{version}";
        }
    }
}