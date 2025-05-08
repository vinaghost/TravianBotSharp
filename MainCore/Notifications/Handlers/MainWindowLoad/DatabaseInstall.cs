using MainCore.Constraints;
using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notifications.Handlers.MainWindowLoad
{
    [Handler]
    public static partial class DatabaseInstall
    {
        private static async ValueTask HandleAsync(
            INotification notification,
            AppDbContext context, IWaitingOverlayViewModel waitingOverlayViewModel,
            CancellationToken cancellationToken)
        {
            await waitingOverlayViewModel.ChangeMessage("loading database");

            var notExist = await context.Database.EnsureCreatedAsync(cancellationToken);

            if (!notExist)
            {
                await Task.Run(context.FillAccountSettings, cancellationToken);
                await Task.Run(context.FillVillageSettings, cancellationToken);
            }
        }
    }
}