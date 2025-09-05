namespace MainCore.Notifications
{
    public record StatusModified(AccountId AccountId, StatusEnums Status) : IAccountNotification;
}
