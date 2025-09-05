using System.ComponentModel.DataAnnotations.Schema;

namespace MainCore.Entities
{
    [Index(nameof(AccountId), nameof(Setting))]
    public class AccountSetting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int AccountId { get; set; }
        public AccountSettingEnums Setting { get; set; }
        public int Value { get; set; }
    }
}
