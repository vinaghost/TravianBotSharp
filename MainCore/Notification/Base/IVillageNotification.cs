namespace MainCore.Notification.Base
{
    public interface IVillageNotification : IAccountNotification
    {
        VillageId VillageId { get; }
    }

    public record VillageNotification(AccountId AccountId, VillageId VillageId) : IVillageNotification;
}