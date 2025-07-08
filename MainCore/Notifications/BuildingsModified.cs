namespace MainCore.Notifications
{
    public record BuildingsModified(VillageId VillageId) : IVillageNotification;
}