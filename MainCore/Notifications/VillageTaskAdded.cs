using MainCore.Tasks.Base;

namespace MainCore.Notifications
{
    public record VillageTaskAdded(VillageTask Task) : INotification;
}