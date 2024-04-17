using Discord;
using Discord.Webhook;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerAlertMessage : INotificationHandler<AccountStop>
    {
        private readonly UnitOfRepository _unitOfRepository;

        public TriggerAlertMessage(UnitOfRepository unitOfRepository)
        {
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(AccountStop notification, CancellationToken cancellationToken)
        {
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
    }
}