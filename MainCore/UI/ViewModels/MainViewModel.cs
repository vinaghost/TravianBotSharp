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

        private readonly IChromeDriverInstaller _chromeDriverInstaller;
        private readonly IChromeManager _chromeManager;
        private readonly IUseragentManager _useragentManager;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MainViewModel(IWaitingOverlayViewModel waitingOverlayViewModel, IChromeDriverInstaller chromeDriverInstaller, IChromeManager chromeManager, IUseragentManager useragentManager, IServiceScopeFactory serviceScopeFactory)
        {
            _waitingOverlayViewModel = waitingOverlayViewModel;
            _chromeDriverInstaller = chromeDriverInstaller;
            _chromeManager = chromeManager;
            _useragentManager = useragentManager;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [ReactiveCommand]
        private async Task Load()
        {
            await _waitingOverlayViewModel.Show();

            await _waitingOverlayViewModel.ChangeMessage("installing chrome driver");
            await Task.Run(_chromeDriverInstaller.Install);

            await _waitingOverlayViewModel.ChangeMessage("installing chrome extension");
            await Task.Run(_chromeManager.LoadExtension);

            await _waitingOverlayViewModel.ChangeMessage("loading chrome useragent");
            await Task.Run(_useragentManager.Load);

            await _waitingOverlayViewModel.ChangeMessage("loading database");
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var notExist = await context.Database.EnsureCreatedAsync();

                if (!notExist)
                {
                    await Task.Run(context.FillAccountSettings);
                    await Task.Run(context.FillVillageSettings);
                }
            }

            await _waitingOverlayViewModel.ChangeMessage("loading program layout");
            MainLayoutViewModel = Locator.Current.GetService<MainLayoutViewModel>()!;

            await MainLayoutViewModel.Load();
            await _waitingOverlayViewModel.Hide();
        }

        [ReactiveCommand]
        private async Task Unload()
        {
            await Task.Run(_chromeManager.Shutdown);

            var path = Path.Combine(AppContext.BaseDirectory, "Plugins");
            if (Directory.Exists(path))
            {
                await Task.Run(() => Directory.Delete(path, true));
            }
            await Task.Run(_useragentManager.Dispose);
        }
    }
}