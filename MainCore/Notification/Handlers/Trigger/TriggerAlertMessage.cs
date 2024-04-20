using Discord;
using Discord.Webhook;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;
using Polly;
using RestSharp;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerAlertMessage : INotificationHandler<AccountStop>
    {
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IRestClientManager _restClientManager;
        private readonly ILogService _logService;
        private readonly ITaskManager _taskManager;
        private readonly IChromeManager _chromeManager;

        public TriggerAlertMessage(UnitOfRepository unitOfRepository, IRestClientManager restClientManager, ILogService logService, ITaskManager taskManager, IChromeManager chromeManager)
        {
            _unitOfRepository = unitOfRepository;
            _restClientManager = restClientManager;
            _logService = logService;
            _taskManager = taskManager;
            _chromeManager = chromeManager;
        }

        public async Task Handle(AccountStop notification, CancellationToken cancellationToken)
        {
            var access = _unitOfRepository.AccountRepository.GetAccess(notification.AccountId);
            var client = _restClientManager.Get(access);

            try
            {
                await Excute(client);
            }
            catch
            {
                var logger = _logService.GetLogger(notification.AccountId);
                var retryPolicy = Policy
               .Handle<Exception>()
               .WaitAndRetryAsync(
                   retryCount: 10,
                   sleepDurationProvider: times => TimeSpan.FromMinutes(2 * times),
                   onRetry: (_, _, retryCount, _) =>
                   {
                       logger.Warning("There is no internet connection. Retry after {time} mins", 2 * retryCount);
                   });

                await retryPolicy
                    .ExecuteAndCaptureAsync(() => Excute(client));

                logger.Information("Internet connection is back");
                var chromeBrowser = _chromeManager.Get(notification.AccountId);
                var account = _unitOfRepository.AccountRepository.Get(notification.AccountId);
                await chromeBrowser.Navigate($"{account.Server}dorf1.php", cancellationToken);
                await _taskManager.SetStatus(notification.AccountId, Common.Enums.StatusEnums.Online);
                return;
            }

            var enable = _unitOfRepository.AccountSettingRepository.GetBooleanByName(notification.AccountId, Common.Enums.AccountSettingEnums.EnableStopAlert);
            if (!enable) return;

            await AlertDiscord(notification.AccountId);
        }

        private async Task AlertDiscord(AccountId accountId)
        {
            var enable = _unitOfRepository.AccountSettingRepository.GetBooleanByName(accountId, Common.Enums.AccountSettingEnums.EnableDiscordAlert);
            if (!enable) return;

            var account = _unitOfRepository.AccountRepository.Get(accountId);

            var webhookUrl = _unitOfRepository.AccountInfoRepository.GetDiscordWebhookUrl(accountId);
            using var client = new DiscordWebhookClient(webhookUrl);
            var embed = new EmbedBuilder
            {
                Title = $"Server: {account.Server}",
                Description = $"Username: {account.Username}",
            };

            await client.SendMessageAsync(text: "@here Account is stopping", embeds: new[] { embed.Build() });
        }

        private async Task Excute(RestClient client)
        {
            var request = new RestRequest
            {
                Method = Method.Get,
            };

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful) throw new Exception();
        }
    }
}