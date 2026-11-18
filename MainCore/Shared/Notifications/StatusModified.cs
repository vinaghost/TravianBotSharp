namespace MainCore.Shared.Notifications
{
    public record StatusModified(AccountId AccountId, StatusEnums Status) : IAccountNotification;
}