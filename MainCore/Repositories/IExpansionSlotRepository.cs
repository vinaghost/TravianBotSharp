using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IExpansionSlotRepository
    {
        bool IsDefaultExpansionSlot(VillageId villageId);
        bool IsSlotAvailable(VillageId villageId);
        void Update(VillageId villageId, List<ExpansionSlotDto> dtos);
    }
}