using FluentResults;
using MainCore.Common.Enums;
using MainCore.DTO;
using MainCore.Entities;

namespace MainCore.Repositories
{
    public interface IHeroItemRepository
    {
        IList<HeroItemEnums> Get(AccountId accountId);

        List<HeroItemDto> GetItems(AccountId accountId);

        Result IsEnoughResource(AccountId accountId, long[] requiredResource);

        void Update(AccountId accountId, List<HeroItemDto> dtos);
    }
}