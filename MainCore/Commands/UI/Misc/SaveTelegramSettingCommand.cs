using MainCore.Constraints;

namespace MainCore.Commands.UI.Misc
{
    [Handler]
    public static partial class SaveTelegramSettingCommand
    {
        public sealed record Command(AccountId AccountId, bool IsEnabled, string BotToken, string ChatId) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var (accountId, isEnabled, botToken, chatId) = command;
            var entity = context.TelegramSettings.FirstOrDefault(x => x.AccountId == accountId.Value);
            if (entity is null)
            {
                context.TelegramSettings.Add(new TelegramSetting
                {
                    AccountId = accountId.Value,
                    IsEnabled = isEnabled,
                    BotToken = botToken,
                    ChatId = chatId,
                });
            }
            else
            {
                entity.IsEnabled = isEnabled;
                entity.BotToken = botToken;
                entity.ChatId = chatId;
            }
            context.SaveChanges();
        }
    }
}
