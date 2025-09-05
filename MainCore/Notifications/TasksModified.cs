namespace MainCore.Notifications
{
    public record TasksModified(AccountId AccountId) : IAccountNotification;
}
