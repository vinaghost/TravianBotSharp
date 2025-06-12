using System.ComponentModel.DataAnnotations.Schema;

namespace MainCore.Entities
{
    public class TelegramSetting
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int AccountId { get; set; }
        public bool IsEnabled { get; set; }
        public string BotToken { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
    }
}
