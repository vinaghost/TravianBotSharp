namespace MainCore.Tasks.Constraints
{
    public interface IVillageTask : IAccountTask
    {
        VillageId VillageId { get; }
    }
}