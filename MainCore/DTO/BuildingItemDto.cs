using MainCore.Common.Enums;
using MainCore.Entities;

namespace MainCore.DTO
{
    public class BuildingItemDto
    {
        public BuildingId Id { get; set; }
        public int Location { get; set; }
        public BuildingEnums Type { get; set; }
        public int Level { get; set; }
        public int QueueLevel { get; set; } = 0;
        public int JobLevel { get; set; } = 0;
    }
}