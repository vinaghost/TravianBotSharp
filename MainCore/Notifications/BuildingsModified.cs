namespace MainCore.Notifications
{
    public record BuildingsModified(AccountId AccountId, VillageId VillageId) : IAccountVillageNotification;
}