namespace MainCore.DTO
{
    public class HeroItemDto
    {
        public HeroItemEnums Type { get; set; }
        public int Amount { get; set; }
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class HeroItemMapper
    {
        public static HeroItem ToEntity(this HeroItemDto dto, AccountId accountId)
        {
            var entity = dto.ToEntity();
            entity.AccountId = accountId.Value;
            return entity;
        }

        [MapperIgnoreTarget(nameof(HeroItem.Id))]
        [MapperIgnoreTarget(nameof(HeroItem.AccountId))]
        public static partial void To(this HeroItemDto dto, HeroItem entity);

        [MapperIgnoreTarget(nameof(HeroItem.Id))]
        [MapperIgnoreTarget(nameof(HeroItem.AccountId))]
        private static partial HeroItem ToEntity(this HeroItemDto dto);
    }
}