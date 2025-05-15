namespace MainCore.Constraints
{
    public interface IConstraint;

    public interface IAccountConstraint
    {
        AccountId AccountId { get; }
    }

    public record AccountConstraint(AccountId AccountId) : IAccountConstraint;

    public interface IVillageConstraint
    {
        VillageId VillageId { get; }
    }

    public interface IAccountVillageConstraint : IAccountConstraint, IVillageConstraint;
}