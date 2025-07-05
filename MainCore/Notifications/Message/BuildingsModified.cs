using MainCore.Constraints;

namespace MainCore.Notifications.Message
{
    public record BuildingsModified(VillageId VillageId) : IVillageNotification;
}