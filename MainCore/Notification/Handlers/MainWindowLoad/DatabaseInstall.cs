using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Notification.Handlers.MainWindowLoad
{
    public class DatabaseInstall : INotificationHandler<MainWindowLoaded>, IEnableLogger
    {
        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public DatabaseInstall(IDbContextFactory<AppDbContext> contextFactory, IWaitingOverlayViewModel waitingOverlayViewModel)
        {
            _contextFactory = contextFactory;
            _waitingOverlayViewModel = waitingOverlayViewModel;
        }

        public async Task Handle(MainWindowLoaded notification, CancellationToken cancellationToken)
        {
            await _waitingOverlayViewModel.ChangeMessage("loading database");
            this.Log().Info("Loading database");
            using var context = await _contextFactory.CreateDbContextAsync();
            var notExist = await context.Database.EnsureCreatedAsync(cancellationToken);
            if (!notExist)
            {
                await Task.Run(context.FillAccountSettings, cancellationToken);
                await Task.Run(context.FillVillageSettings, cancellationToken);
            }
            this.Log().Info("Database loaded");
        }
    }
}