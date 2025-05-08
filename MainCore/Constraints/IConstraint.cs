namespace MainCore.Constraints
{
    public interface IConstraint;

    public interface IAccountConstraint
    {
        AccountId AccountId { get; }
    }

    public interface IVillageConstraint
    {
        VillageId VillageId { get; }
    }

    public interface IAccountVillageConstraint : IAccountConstraint, IVillageConstraint;
}