namespace MainCore.Queries.Base
{
    public interface IAccountQuery : IQuery
    {
        AccountId AccountId { get; }
    }
}