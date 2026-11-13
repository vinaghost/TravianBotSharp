namespace MainCore.Shared.Notifications
{
    public record FarmsModified(AccountId AccountId) : IAccountNotification;
}