using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IAdventureRepository
    {
        void Update(AccountId accountId, List<AdventureDto> dtos);
    }
}