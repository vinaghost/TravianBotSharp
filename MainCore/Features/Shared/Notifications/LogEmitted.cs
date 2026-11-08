using Serilog.Events;

namespace MainCore.Features.Shared.Notifications
{
    public record LogEmitted(AccountId AccountId, LogEvent LogEvent) : IAccountNotification;
}