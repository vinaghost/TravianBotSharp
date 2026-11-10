namespace MainCore.Shared.Notifications
{
    public record TasksModified(AccountId AccountId) : IAccountNotification;
}