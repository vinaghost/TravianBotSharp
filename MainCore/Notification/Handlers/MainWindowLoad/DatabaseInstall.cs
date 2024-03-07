using MainCore.Infrasturecture.Persistence;
using MainCore.Notification.Message;
using MainCore.UI.ViewModels.UserControls;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Notification.Handlers.MainWindowLoad
{
    public class DatabaseInstall : INotificationHandler<MainWindowLoaded>
    {
        private readonly WaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public DatabaseInstall(IDbContextFactory<AppDbContext> contextFactory, WaitingOverlayViewModel waitingOverlayViewModel)
        {
            _contextFactory = contextFactory;
            _waitingOverlayViewModel = waitingOverlayViewModel;
        }

        public async Task Handle(MainWindowLoaded notification, CancellationToken cancellationToken)
        {
            await _waitingOverlayViewModel.ChangeMessage("loading database");
            using var context = _contextFactory.CreateDbContext();
            var notExist = await context.Database.EnsureCreatedAsync(cancellationToken);
            if (!notExist)
            {
                await Task.Run(context.FillAccountSettings, cancellationToken);
                await Task.Run(context.FillVillageSettings, cancellationToken);
                await Task.Run(context.FillHero, cancellationToken);
            }
            else
            {
                await Task.Run(context.AddVersionInfo, cancellationToken);
            }
        }
    }
}