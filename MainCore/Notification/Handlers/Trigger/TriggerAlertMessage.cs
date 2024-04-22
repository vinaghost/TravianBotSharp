using Discord;
using Discord.Webhook;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;
using Polly;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerAlertMessage : INotificationHandler<AccountStop>
    {
        private readonly UnitOfRepository _unitOfRepository;
        private readonly ILogService _logService;
        private readonly ITaskManager _taskManager;
        private readonly IChromeManager _chromeManager;

        public TriggerAlertMessage(UnitOfRepository unitOfRepository, ILogService logService, ITaskManager taskManager, IChromeManager chromeManager)
        {
            _unitOfRepository = unitOfRepository;
            _logService = logService;
            _taskManager = taskManager;
            _chromeManager = chromeManager;
        }

        public async Task Handle(AccountStop notification, CancellationToken cancellationToken)
        {
            var access = _unitOfRepository.AccountRepository.GetAccess(notification.AccountId);
            var chromeBrowser = _chromeManager.Get(notification.AccountId);
            var account = _unitOfRepository.AccountRepository.Get(notification.AccountId);
            try
            {
                _ = chromeBrowser.Html;
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

                var url = $"{account.Server}dorf1.php";
                await retryPolicy
                    .ExecuteAndCaptureAsync(() => Excute(chromeBrowser, url));

                await chromeBrowser.Navigate(url, default);
                logger.Information("Internet connection is back");
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

        private static async Task Excute(IChromeBrowser chromeBrowser, string url)
        {
            var result = await chromeBrowser.Navigate(url, default);
            if (result.IsFailed) throw new Exception();
            result = await chromeBrowser.WaitPageLoaded(default);
            if (result.IsFailed) throw new Exception();
            _ = chromeBrowser.Html;
        }
    }
}