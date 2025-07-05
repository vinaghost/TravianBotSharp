using MainCore.Constraints;

namespace MainCore.Notifications.Message
{
    public record FarmsModified(AccountId AccountId) : IAccountNotification;
}