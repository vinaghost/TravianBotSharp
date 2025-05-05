namespace MainCore.Queries.Base
{
    public interface IVillageQuery : IAccountQuery
    {
        VillageId VillageId { get; }
    }
}