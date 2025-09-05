using StronglyTypedIds;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainCore.Entities
{
    public class Building
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public BuildingEnums Type { get; set; }
        public int Level { get; set; }
        public bool IsUnderConstruction { get; set; }
        public int Location { get; set; }
        public int VillageId { get; set; }
    }

    [StronglyTypedId]
    public partial struct BuildingId
    { }
}
