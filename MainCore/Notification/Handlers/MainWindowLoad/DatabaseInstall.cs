using MainCore.Notification.Base;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notification.Handlers.MainWindowLoad
{
    [Handler]
    public static partial class DatabaseInstall
    {
        private static async ValueTask HandleAsync(
            INotification notification,
            IDbContextFactory<AppDbContext> contextFactory, IWaitingOverlayViewModel waitingOverlayViewModel,
            CancellationToken cancellationToken)
        {
            await waitingOverlayViewModel.ChangeMessage("loading database");

            using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var notExist = await context.Database.EnsureCreatedAsync(cancellationToken);

            if (!notExist)
            {
                await Task.Run(context.FillAccountSettings, cancellationToken);
                await Task.Run(context.FillVillageSettings, cancellationToken);
            }
        }
    }
}