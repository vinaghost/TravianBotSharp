namespace MainCore.Notifications
{
    public record VillagesModified(AccountId AccountId) : IAccountNotification;
}