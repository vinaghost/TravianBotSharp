namespace MainCore.Shared.Notifications
{
    public record JobsModified(AccountId AccountId, VillageId VillageId) : IAccountVillageNotification;
}