using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace MainCore.Services
{
    [RegisterSingleton<ITelegramService, TelegramService>]
    public sealed class TelegramService : ITelegramService
    {
        private readonly ILogger _logger;
        private readonly TelegramBotClient? _client;
        private readonly string? _chatId;

        public TelegramService(ILogger logger)
        {
            _logger = logger.ForContext<TelegramService>();
            var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            _chatId = Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID");
            if (!string.IsNullOrEmpty(token))
            {
                _client = new TelegramBotClient(token);
            }
        }

        public async Task SendText(string message, AccountId accountId)
        {
            if (_client is null) return;
            if (string.IsNullOrEmpty(_chatId)) return;

            try
            {
                await _client.SendTextMessageAsync(_chatId, message);
            }
            catch (Exception ex) when (ex is ApiRequestException or HttpRequestException)
            {
                _logger.Warning(ex, "Failed to send telegram message");
            }
        }
    }
}
