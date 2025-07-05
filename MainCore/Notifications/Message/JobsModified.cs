using MainCore.Constraints;

namespace MainCore.Notifications.Message
{
    public record JobsModified(VillageId VillageId) : IVillageNotification;
}