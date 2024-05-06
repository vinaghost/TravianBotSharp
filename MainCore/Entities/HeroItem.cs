using System.ComponentModel.DataAnnotations.Schema;

namespace MainCore.Entities
{
    [Index(nameof(Type))]
    public class HeroItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public HeroItemEnums Type { get; set; }
        public int Amount { get; set; }
        public int AccountId { get; set; }
    }
}