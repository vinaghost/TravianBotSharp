using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IHeroRepository
    {
        void Update(AccountId accountId, HeroDto dto);
    }
}