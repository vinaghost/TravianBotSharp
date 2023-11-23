using MainCore.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainCore.Entities
{
    public class QueueBuilding
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int VillageId { get; set; }
        public int Position { get; set; }
        public int Location { get; set; } = -1;
        public BuildingEnums Type { get; set; }
        public int Level { get; set; }
        public DateTime CompleteTime { get; set; }
    }
}