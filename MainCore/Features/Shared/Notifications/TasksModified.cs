namespace MainCore.Features.Shared.Notifications
{
    public record TasksModified(AccountId AccountId) : IAccountNotification;
}