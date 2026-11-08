namespace MainCore.Features.Shared.Notifications
{
    public record StatusModified(AccountId AccountId, StatusEnums Status) : IAccountNotification;
}