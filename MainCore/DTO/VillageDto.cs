#nullable disable

namespace MainCore.DTO
{
    public class VillageDto
    {
        public VillageId Id { get; set; }
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public bool IsActive { get; set; }
        public bool IsUnderAttack { get; set; }
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class VillageMapper
    {
        public static Village ToEntity(this VillageDto dto, AccountId accountId)
        {
            var entity = ToEntity(dto);
            entity.AccountId = accountId.Value;
            return entity;
        }

        [MapperIgnoreTarget(nameof(Village.AccountId))]
        [MapperIgnoreTarget(nameof(Village.Buildings))]
        [MapperIgnoreTarget(nameof(Village.QueueBuildings))]
        [MapperIgnoreTarget(nameof(Village.Jobs))]
        [MapperIgnoreTarget(nameof(Village.Storage))]
        [MapperIgnoreTarget(nameof(Village.VillageSetting))]
        private static partial Village ToEntity(this VillageDto dto);

        [MapperIgnoreTarget(nameof(Village.Id))]
        [MapperIgnoreTarget(nameof(Village.AccountId))]
        [MapperIgnoreTarget(nameof(Village.Buildings))]
        [MapperIgnoreTarget(nameof(Village.QueueBuildings))]
        [MapperIgnoreTarget(nameof(Village.Jobs))]
        [MapperIgnoreTarget(nameof(Village.Storage))]
        [MapperIgnoreTarget(nameof(Village.VillageSetting))]
        public static partial void To(this VillageDto dto, Village entity);

        public static partial VillageDto ToDto(this Village dto);

        public static partial IQueryable<VillageDto> ToDto(this IQueryable<Village> entities);

        private static int ToInt(this VillageId villageId) => villageId.Value;

        private static VillageId ToVillageId(this int value) => new(value);
    }
}