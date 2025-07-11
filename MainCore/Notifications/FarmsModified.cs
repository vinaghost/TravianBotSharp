namespace MainCore.Notifications
{
    public record FarmsModified(AccountId AccountId) : IAccountNotification;
}