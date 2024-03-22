using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IExpansionSlotRepository
    {
        void Update(VillageId villageId, List<ExpansionSlotDto> dtos);
    }
}