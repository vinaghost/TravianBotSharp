using System.ComponentModel.DataAnnotations.Schema;

namespace MainCore.Entities
{
    public class Storage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int VillageId { get; set; }
        public long Wood { get; set; }
        public long Clay { get; set; }
        public long Iron { get; set; }
        public long Crop { get; set; }
        public long ProductionWood { get; set; }
        public long ProductionClay { get; set; }
        public long ProductionIron { get; set; }
        public long ProductionCrop { get; set; }
        public long Warehouse { get; set; }
        public long Granary { get; set; }
        public long FreeCrop { get; set; }
    }
}