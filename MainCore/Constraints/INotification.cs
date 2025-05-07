namespace MainCore.Constraints
{
    public interface INotification;

    public interface IAccountNotification : INotification, IAccountConstraint;

    public interface IVillageNotification : INotification, IVillageConstraint;

    public record Notification() : INotification;
    public record VillageNotification(AccountId AccountId, VillageId VillageId) : IVillageNotification;
}