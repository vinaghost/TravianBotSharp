namespace MainCore.Shared.Notifications
{
    public record VillageTaskAdded(VillageTask Task) : INotification;
}