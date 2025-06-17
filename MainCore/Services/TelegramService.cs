using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using MainCore.Queries;

namespace MainCore.Services
{
    [RegisterSingleton<ITelegramService, TelegramService>]
    public sealed class TelegramService : ITelegramService
    {
        private readonly ILogger _logger;
        private readonly ICustomServiceScopeFactory _scopeFactory;
        private readonly Dictionary<AccountId, (TelegramBotClient Client, string ChatId)> _clients = new();

        public event Action<AccountId, string> CommandReceived = delegate { };

        public TelegramService(ILogger logger, ICustomServiceScopeFactory scopeFactory)
        {
            _logger = logger.ForContext<TelegramService>();
            _scopeFactory = scopeFactory;
        }

        private async Task<(TelegramBotClient? client, string chatId)> GetClient(AccountId accountId)
        {
            if (_clients.TryGetValue(accountId, out var data))
            {
                return data;
            }

            using var scope = _scopeFactory.CreateScope(accountId);
            var getSettingQuery = scope.ServiceProvider.GetRequiredService<GetTelegramSettingQuery.Handler>();
            var setting = await getSettingQuery.HandleAsync(new(accountId));
            if (setting is null) return (null, string.Empty);
            if (string.IsNullOrEmpty(setting.BotToken) || string.IsNullOrEmpty(setting.ChatId)) return (null, string.Empty);

            var client = new TelegramBotClient(setting.BotToken);
            client.StartReceiving((bot, update, ct) => HandleUpdate(accountId, update, ct), HandleError);
            data = (client, setting.ChatId);
            _clients[accountId] = data;
            return data;
        }

        private Task HandleUpdate(AccountId accountId, Update update, CancellationToken token)
        {
            if (update.Message is not { Text: { } text, Chat.Id: var chatId }) return Task.CompletedTask;
            if (_clients.TryGetValue(accountId, out var data) && data.ChatId == chatId.ToString())
            {
                CommandReceived.Invoke(accountId, text);
                _logger.Information("Received telegram command for {AccountId}: {Text}", accountId, text);
            }
            return Task.CompletedTask;
        }

        private Task HandleError(ITelegramBotClient bot, Exception ex, CancellationToken token)
        {
            if (ex is ApiRequestException or HttpRequestException)
                _logger.Warning(ex, "Telegram polling error");
            return Task.CompletedTask;
        }

        public async Task SendText(string message, AccountId accountId)
        {
            var (client, chatId) = await GetClient(accountId);
            if (client is null) return;
            if (string.IsNullOrEmpty(chatId)) return;

            try
            {
                await client.SendMessage(chatId, message);
            }
            catch (Exception ex) when (ex is ApiRequestException or HttpRequestException)
            {
                _logger.Warning(ex, "Failed to send telegram message");
            }
        }
    }
}

