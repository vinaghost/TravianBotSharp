using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.UI.ViewModels
{
    [RegisterSingleton<MainViewModel>]
    public partial class MainViewModel : ViewModelBase
    {
        [Reactive]
        private MainLayoutViewModel _mainLayoutViewModel = null!;

        private readonly IWaitingOverlayViewModel _waitingOverlayViewModel;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MainViewModel(IWaitingOverlayViewModel waitingOverlayViewModel, IServiceScopeFactory serviceScopeFactory)
        {
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [ReactiveCommand]
        private async Task Load()
        {
            await _waitingOverlayViewModel.Show();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                await _waitingOverlayViewModel.ChangeMessage("installing chrome driver");
                var chromeDriverInstaller = scope.ServiceProvider.GetRequiredService<IChromeDriverInstaller>();
                await Task.Run(chromeDriverInstaller.Install);

                await _waitingOverlayViewModel.ChangeMessage("installing chrome extension");
                var chromeManager = scope.ServiceProvider.GetRequiredService<IChromeManager>();
                await Task.Run(chromeManager.LoadExtension);

                await _waitingOverlayViewModel.ChangeMessage("loading chrome useragent");
                var useragentManager = scope.ServiceProvider.GetRequiredService<IUseragentManager>();
                await Task.Run(useragentManager.Load);

                await _waitingOverlayViewModel.ChangeMessage("loading database");

                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var notExist = await context.Database.EnsureCreatedAsync();

                if (!notExist)
                {
                    await Task.Run(context.FillAccountSettings);
                    await Task.Run(context.FillVillageSettings);
                    await Task.Run(context.EnsureStorageProductionColumns);
                }

                await _waitingOverlayViewModel.ChangeMessage("loading program layout");
                MainLayoutViewModel = scope.ServiceProvider.GetRequiredService<MainLayoutViewModel>();
                await MainLayoutViewModel.Load();
            }

            await _waitingOverlayViewModel.Hide();
        }

        [ReactiveCommand]
        private async Task Unload()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var chromeManager = scope.ServiceProvider.GetRequiredService<IChromeManager>();
                await chromeManager.Shutdown();

                var path = Path.Combine(AppContext.BaseDirectory, "Plugins");
                if (Directory.Exists(path))
                {
                    await Task.Run(() => Directory.Delete(path, true));
                }

                var useragentManager = scope.ServiceProvider.GetRequiredService<IUseragentManager>();
                await Task.Run(useragentManager.Dispose);
            }
        }
    }
}