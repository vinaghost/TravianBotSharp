using StronglyTypedIds;
using System.ComponentModel.DataAnnotations.Schema;

namespace MainCore.Entities
{
    public class Account
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Username { get; set; }
        public string Server { get; set; }

        public AccountInfo Info { get; set; }
        public Hero Hero { get; set; }

        public ICollection<AccountSetting> Settings { get; set; }
        public ICollection<Access> Accesses { get; set; }
        public ICollection<Village> Villages { get; set; }

        public ICollection<HeroItem> HeroItems { get; set; }
        public ICollection<Farm> FarmLists { get; set; }

        public ICollection<Adventure> Adventures { get; set; }
    }

    [StronglyTypedId]
    public partial struct AccountId
    { }
}