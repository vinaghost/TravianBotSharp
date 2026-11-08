namespace MainCore.Features.Shared.Notifications
{
    public record VillageTaskAdded(VillageTask Task) : INotification;
}