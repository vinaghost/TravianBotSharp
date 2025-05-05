using Serilog.Events;

namespace MainCore.Services
{
    public interface ILogService
    {
        LinkedList<LogEvent> GetLog(AccountId accountId);

        Serilog.ILogger GetLogger(AccountId accountId);

        void Shutdown();
    }
}