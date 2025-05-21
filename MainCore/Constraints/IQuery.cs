namespace MainCore.Constraints
{
    public interface IQuery : IConstraint;

    public interface IAccountQuery : IQuery, IAccountConstraint;

    public interface IVillageQuery : IQuery, IVillageConstraint;

    public interface IAccountVillageQuery : IQuery, IAccountVillageConstraint;
}