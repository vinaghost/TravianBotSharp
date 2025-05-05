namespace MainCore.Tasks.Base
{
    public interface IVillageTask : IAccountTask
    {
        VillageId VillageId { get; }
    }
}