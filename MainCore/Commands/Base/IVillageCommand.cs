namespace MainCore.Commands.Base
{
    public interface IVillageCommand
    {
        public Task<Result> Execute(AccountId accountId, VillageId villageId, CancellationToken cancellationToken);
    }
}