using Microsoft.Extensions.Hosting;

namespace MainCore.Services
{
    public sealed class TelegramCommandHostedService : IHostedService
    {
        // Just resolving TelegramCommandService triggers event subscription
        private readonly TelegramCommandService _commandService;

        public TelegramCommandHostedService(TelegramCommandService commandService)
        {
            _commandService = commandService;
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

