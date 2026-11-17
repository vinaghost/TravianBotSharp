namespace MainCore.Shared.Notifications
{
    public record VillagesModified(AccountId AccountId) : IAccountNotification;
}