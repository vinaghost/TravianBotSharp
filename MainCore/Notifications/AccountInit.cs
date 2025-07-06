namespace MainCore.Notifications
{
    public record AccountInit(AccountId AccountId) : IAccountNotification;
}