using MainCore.Common.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainCore.Entities
{
    [Index(nameof(VillageId), nameof(Setting))]
    public class VillageSetting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int VillageId { get; set; }
        public VillageSettingEnums Setting { get; set; }
        public int Value { get; set; }
    }
}