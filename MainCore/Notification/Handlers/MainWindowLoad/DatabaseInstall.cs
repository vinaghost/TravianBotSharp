using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.Logging;

namespace MainCore.Notification.Handlers.MainWindowLoad
{
    [Handler]
    public static partial class DatabaseInstall
    {
        private static async ValueTask HandleAsync(

            MainWindowLoaded notification,
            IDbContextFactory<AppDbContext> contextFactory, IWaitingOverlayViewModel waitingOverlayViewModel, ILogger<MainWindowLoaded> logger,
            CancellationToken cancellationToken)
        {
            await waitingOverlayViewModel.ChangeMessage("loading database");
            logger.LogInformation("Loading database");

            using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var notExist = await context.Database.EnsureCreatedAsync(cancellationToken);

            if (!notExist)
            {
                await Task.Run(context.FillAccountSettings, cancellationToken);
                await Task.Run(context.FillVillageSettings, cancellationToken);
            }

            logger.LogInformation("Database loaded");
        }
    }
}