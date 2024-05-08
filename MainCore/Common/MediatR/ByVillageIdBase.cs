namespace MainCore.Common.MediatR
{
    public class ByVillageIdBase
    {
        public VillageId VillageId { get; }

        public ByVillageIdBase(VillageId villageId)
        {
            VillageId = villageId;
        }
    }
}