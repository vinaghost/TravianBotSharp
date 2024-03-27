using MainCore.Common.Enums;

namespace MainCore.Entities
{
    public class Troop
    {
        public int Id { get; set; }
        public TroopEnums Type { get; set; }
        public int Level { get; set; } = -1;

        public int VillageId { get; set; }
    }
}