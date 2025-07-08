namespace MainCore.Constraints
{
    public interface IConstraint;

    public record Constraint : IConstraint;

    public interface IAccountConstraint
    {
        AccountId AccountId { get; }
    }

    public record AccountConstraint(AccountId AccountId) : IAccountConstraint;

    public interface IVillageConstraint
    {
        VillageId VillageId { get; }
    }

    public interface IAccountVillageConstraint : IAccountConstraint, IVillageConstraint
    {
        void Deconstruct(out AccountId accountId, out VillageId villageId);
    }

    public record AccountVillageConstraint(AccountId AccountId, VillageId VillageId) : IAccountVillageConstraint;
}