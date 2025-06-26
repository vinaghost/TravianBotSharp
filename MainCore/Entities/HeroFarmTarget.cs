using System.ComponentModel.DataAnnotations.Schema;

namespace MainCore.Entities
{
    public class HeroFarmTarget
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public string OasisType { get; set; } = "Unknown";
        public string Animal { get; set; } = "Unknown";
        public int Resource { get; set; }
        public DateTime LastSend { get; set; } = DateTime.MinValue;
        public int AccountId { get; set; }
    }
}