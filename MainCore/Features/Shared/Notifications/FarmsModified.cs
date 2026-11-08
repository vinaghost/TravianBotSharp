namespace MainCore.Features.Shared.Notifications
{
    public record FarmsModified(AccountId AccountId) : IAccountNotification;
}