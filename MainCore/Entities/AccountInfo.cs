using System.ComponentModel.DataAnnotations.Schema;

namespace MainCore.Entities
{
    public class AccountInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public TribeEnums Tribe { get; set; }
        public int Gold { get; set; }
        public int Silver { get; set; }
        public bool HasPlusAccount { get; set; }
        public int AccountId { get; set; }
    }
}
