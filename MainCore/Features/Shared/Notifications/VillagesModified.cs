namespace MainCore.Features.Shared.Notifications
{
    public record VillagesModified(AccountId AccountId) : IAccountNotification;
}