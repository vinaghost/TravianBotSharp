using MainCore.DTO;

namespace MainCore.Repositories
{
    public interface IHeroItemRepository
    {
        Result IsEnoughResource(AccountId accountId, long[] requiredResource);
        void Update(AccountId accountId, List<HeroItemDto> dtos);
    }
}