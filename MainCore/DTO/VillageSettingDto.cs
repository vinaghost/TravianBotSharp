using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class VillageSettingDto
    {
        public int Id { get; set; }
        public VillageSettingEnums Setting { get; set; }
        public int Value { get; set; }
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class VillageSettingMapper
    {
        public static VillageSetting ToEntity(this VillageSettingDto dto, VillageId villageId)
        {
            var entity = ToEntity(dto);
            entity.VillageId = villageId.Value;
            return entity;
        }

        [MapperIgnoreTarget(nameof(VillageSetting.VillageId))]
        private static partial VillageSetting ToEntity(this VillageSettingDto dto);

        public static partial VillageSettingDto ToDto(this VillageSetting dto);
    }
}