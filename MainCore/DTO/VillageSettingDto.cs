using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class VillageSettingDto
    {
        public int Id { get; set; }
        public VillageSettingEnums Setting { get; set; }
        public int Value { get; set; }
    }

    [Mapper]
    public static partial class VillageSettingMapper
    {
        public static partial VillageSettingDto ToDto(this VillageSetting dto);

        public static VillageSetting ToEntity(this VillageSettingDto dto, VillageId villageId)
        {
            var entity = ToEntity(dto);
            entity.VillageId = villageId.Value;
            return entity;
        }

        private static partial VillageSetting ToEntity(this VillageSettingDto dto);
    }
}