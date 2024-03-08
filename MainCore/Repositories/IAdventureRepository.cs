using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IAdventureRepository
    {
        List<AdventureDto> Get(AccountId accountId);

        void Update(AccountId accountId, List<AdventureDto> dtos);
    }
}