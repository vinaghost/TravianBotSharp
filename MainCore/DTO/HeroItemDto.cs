using MainCore.Common.Enums;
using MainCore.Entities;
using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class HeroItemDto
    {
        public HeroItemEnums Type { get; set; }
        public int Amount { get; set; }
    }

    [Mapper]
    public static partial class HeroItemMapper
    {
        public static HeroItem ToEntity(this HeroItemDto dto, AccountId accountId)
        {
            var entity = dto.ToEntity();
            entity.AccountId = accountId.Value;
            return entity;
        }

        public static partial void To(this HeroItemDto dto, HeroItem entity);

        private static partial HeroItem ToEntity(this HeroItemDto dto);
    }
}