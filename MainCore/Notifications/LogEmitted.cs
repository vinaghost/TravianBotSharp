using Serilog.Events;

namespace MainCore.Notifications
{
    public record LogEmitted(AccountId AccountId, LogEvent LogEvent) : IAccountNotification;
}
