#nullable disable

namespace MainCore.DTO
{
    public class QueueBuildingDto
    {
        public int Position { get; set; }

        public int Location { get; set; } = -1;
        public string Type { get; set; }
        public int Level { get; set; }
        public DateTime CompleteTime { get; set; }
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class QueueBuildingMapper
    {
        public static QueueBuilding ToEntity(this QueueBuildingDto dto, VillageId villageId)
        {
            var entity = dto.ToEntity();
            entity.VillageId = villageId.Value;
            return entity;
        }

        [MapperIgnoreTarget(nameof(QueueBuilding.Id))]
        [MapperIgnoreTarget(nameof(QueueBuilding.VillageId))]
        [MapperIgnoreTarget(nameof(QueueBuilding.Location))]
        public static partial void To(this QueueBuildingDto dto, QueueBuilding entity);

        [MapperIgnoreTarget(nameof(QueueBuilding.Id))]
        [MapperIgnoreTarget(nameof(QueueBuilding.VillageId))]
        private static partial QueueBuilding ToEntity(this QueueBuildingDto dto);

        private static BuildingEnums ToBuildingEnums(string str) => Enum.Parse<BuildingEnums>(str);
    }
}