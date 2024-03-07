using MainCore.Common.Enums;
using MainCore.Entities;
using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class HeroDto
    {
        public int Health { get; set; }
        public HeroStatusEnums Status { get; set; }
    }

    [Mapper]
    public static partial class HeroMapper
    {
        public static Hero ToEntity(this HeroDto dto, AccountId accountId)
        {
            var entity = dto.ToEntity();
            entity.AccountId = accountId.Value;
            return entity;
        }

        public static partial void To(this HeroDto dto, Hero entity);

        private static partial Hero ToEntity(this HeroDto dto);
    }
}