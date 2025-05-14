namespace MainCore.DTO
{
    public class BuildingDto
    {
        public BuildingId Id { get; set; }
        public BuildingEnums Type { get; set; }
        public int Level { get; set; }
        public bool IsUnderConstruction { get; set; }
        public int Location { get; set; }
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class BuildingMapper
    {
        public static Building ToEntity(this BuildingDto dto, VillageId villageId)
        {
            var entity = dto.ToEntity();
            entity.VillageId = villageId.Value;
            return entity;
        }

        [MapperIgnoreTarget(nameof(Building.VillageId))]
        [MapperIgnoreTarget(nameof(Building.Id))]
        private static partial Building ToEntity(this BuildingDto dto);

        [MapperIgnoreTarget(nameof(Building.VillageId))]
        [MapperIgnoreTarget(nameof(Building.Id))]
        public static partial void To(this BuildingDto dto, Building entity);

        public static partial BuildingDto ToDto(this Building entity);

        public static partial IQueryable<BuildingDto> ToDto(this IQueryable<Building> entities);

        private static int ToInt(this BuildingId buildingId) => buildingId.Value;

        private static BuildingId ToBuildingId(this int value) => new(value);
    }
}