using MainCore.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainCore.Entities
{
    public class Hero
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int AccountId { get; set; }

        public int Health { get; set; }
        public HeroStatusEnums Status { get; set; }
    }
}