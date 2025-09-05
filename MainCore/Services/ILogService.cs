using Serilog.Events;

namespace MainCore.Services
{
    public interface ILogService
    {
        Dictionary<AccountId, ILogger> Loggers { get; }

        LinkedList<LogEvent> GetLog(AccountId accountId);

        void Shutdown();
    }
}
