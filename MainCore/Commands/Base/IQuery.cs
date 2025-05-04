namespace MainCore.Commands.Base
{
    public interface IQuery;

    public interface IAccountQuery : IQuery
    {
        AccountId AccountId { get; }
    }

    public interface IVillageQuery : IAccountQuery
    {
        VillageId VillageId { get; }
    }
}