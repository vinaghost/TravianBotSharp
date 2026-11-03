namespace MainCore.Notifications
{
    public record VillageTaskAdded(VillageTask Task) : INotification;
}