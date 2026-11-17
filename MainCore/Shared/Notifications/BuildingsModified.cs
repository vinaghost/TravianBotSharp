namespace MainCore.Shared.Notifications
{
    public record BuildingsModified(AccountId AccountId, VillageId VillageId) : IAccountVillageNotification;
}