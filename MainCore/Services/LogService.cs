using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Reflection;

namespace MainCore.Services
{
    [RegisterSingleton<ILogService, LogService>]
    public sealed class LogService : ILogService
    {
        private readonly Dictionary<AccountId, ILogger> _loggers = [];

        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly LogSink _logSink;

        public LogService(IDbContextFactory<AppDbContext> contextFactory, IServiceProvider serviceProvider, ILogEventSink logSink)
        {
            _contextFactory = contextFactory;
            _serviceProvider = serviceProvider;
            _logSink = logSink as LogSink;
        }

        public void Load()
        {
            Log.Logger = new LoggerConfiguration()
              .ReadFrom.Services(_serviceProvider)
              .WriteTo.Map("Account", "Other", (acc, wt) =>
                    wt.File($"./logs/log-{acc}-.txt",
                            rollingInterval: RollingInterval.Day,
                            outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
              .CreateLogger();
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

        public ILogger GetLogger(AccountId accountId)
        {
            var logger = _loggers.GetValueOrDefault(accountId);
            if (logger is null)
            {
                using var context = _contextFactory.CreateDbContext();
                var account = context.Accounts
                    .Where(x => x.Id == accountId.Value)
                    .First();

                var uri = new Uri(account.Server);
                logger = Log.ForContext("Account", $"{account.Username}_{uri.Host}")
                            .ForContext("AccountId", accountId);
                _loggers.Add(accountId, logger);
                logger.Information("===============> Current version: {Version} <===============", GetVersion());
            }
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