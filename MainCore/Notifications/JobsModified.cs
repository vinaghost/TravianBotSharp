namespace MainCore.Notifications
{
    public record JobsModified(AccountId AccountId, VillageId VillageId) : IAccountVillageNotification;
}