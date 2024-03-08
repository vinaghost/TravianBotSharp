using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IHeroRepository
    {
        HeroDto Get(AccountId accountId);
        void Update(AccountId accountId, HeroDto dto);
    }
}