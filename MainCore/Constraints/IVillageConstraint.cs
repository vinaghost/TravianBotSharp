namespace MainCore.Constraints
{
    public interface IVillageConstraint : IAccountConstraint
    {
        VillageId VillageId { get; }
    }
}