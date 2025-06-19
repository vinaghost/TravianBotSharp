using MainCore.Commands.Update;
using MainCore.Infrasturecture.Persistence;
using MainCore.Parsers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MainCore.Services;

public sealed class PassiveUpdateHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public PassiveUpdateHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            var serviceScopeFactory = scope.ServiceProvider.GetRequiredService<ICustomServiceScopeFactory>();

            var accountIds = context.Accounts.Select(a => new AccountId(a.Id)).ToList();

            foreach (var accountId in accountIds)
            {
                if (taskManager.GetStatus(accountId) == StatusEnums.Offline) continue;

                using var accScope = serviceScopeFactory.CreateScope(accountId);
                var browser = accScope.ServiceProvider.GetRequiredService<IChromeBrowser>();
                var updateStorage = accScope.ServiceProvider.GetRequiredService<UpdateStorageCommand.Handler>();
                var updateBuilding = accScope.ServiceProvider.GetRequiredService<UpdateBuildingCommand.Handler>();

                var html = browser.Html;
                var villageId = VillagePanelParser.GetCurrentVillageId(html);
                if (villageId == VillageId.Empty) continue;

                await updateStorage.HandleAsync(new(accountId, villageId), stoppingToken);

                var url = browser.CurrentUrl;
                if (url.Contains("dorf1") || url.Contains("dorf2"))
                {
                    await updateBuilding.HandleAsync(new(accountId, villageId), stoppingToken);
                }
            }
        }
    }
}
