using Serilog.Events;

namespace MainCore.Shared.Notifications
{
    public record LogEmitted(AccountId AccountId, LogEvent LogEvent) : IAccountNotification;
}