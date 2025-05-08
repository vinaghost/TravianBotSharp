namespace MainCore.Constraints
{
    public interface INotification : IConstraint;

    public interface IAccountNotification : INotification, IAccountConstraint;

    public interface IVillageNotification : INotification, IVillageConstraint;

    public interface IAccountVillageNotification : INotification, IAccountVillageConstraint;

    public sealed record Notification() : INotification;
    public sealed record VillageNotification(AccountId AccountId, VillageId VillageId) : IAccountVillageNotification;
}