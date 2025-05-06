namespace MainCore.Constraints
{
    public interface IQuery;

    public interface IAccountQuery : IQuery, IAccountConstraint;

    public interface IVillageQuery : IQuery, IVillageConstraint;
}