using MainCore.Constraints;

namespace MainCore.Notifications.Message
{
    public record VillagesModified(AccountId AccountId) : IAccountNotification;
}