using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IExpansionSlotRepository
    {
        bool IsDefaultExpansionSlot(VillageId villageId);
        void Update(VillageId villageId, List<ExpansionSlotDto> dtos);
    }
}