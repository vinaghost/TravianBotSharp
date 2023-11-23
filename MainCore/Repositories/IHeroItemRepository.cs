using FluentResults;
using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IHeroItemRepository
    {
        Result IsEnoughResource(AccountId accountId, long[] requiredResource);
        void Update(AccountId accountId, List<HeroItemDto> dtos);
    }
}