using MainCore.Entities;
using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.Models.Input
{
    public partial class TelegramSettingInput : ViewModelBase
    {
        [Reactive]
        private bool _isEnabled;
        [Reactive]
        private string _botToken = string.Empty;
        [Reactive]
        private string _chatId = string.Empty;

        public void Set(TelegramSetting? setting)
        {
            if (setting is null)
            {
                IsEnabled = false;
                BotToken = string.Empty;
                ChatId = string.Empty;
            }
            else
            {
                IsEnabled = setting.IsEnabled;
                BotToken = setting.BotToken;
                ChatId = setting.ChatId;
            }
        }

        public (bool IsEnabled, string BotToken, string ChatId) Get()
        {
            return (IsEnabled, BotToken, ChatId);
        }
    }
}

