using MainCore.UI.ViewModels.Abstract;
using MainCore.UI.ViewModels.UserControls;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Primitives.Extensions;
using ReactiveUI.Primitives.Signals;

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
                var useragentManager = scope.ServiceProvider.GetRequiredService<IUseragentManager>();

                var installChromeDriver = Signal.Start(chromeDriverInstaller.Install, RxSchedulers.TaskpoolScheduler);
                var loadUseragent = Signal.Start(useragentManager.Load, RxSchedulers.TaskpoolScheduler);
                var chromeManager = scope.ServiceProvider.GetRequiredService<IChromeManager>();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var loadDatabase = Signal.Start(async () =>
                {
                    var notExist = await context.Database.EnsureCreatedAsync();

                    if (!notExist)
                    {
                        context.FillAccountSettings();
                        context.FillVillageSettings();

                        context.QueueBuildings
                            .Where(x => x.Level == -1)
                            .ExecuteDelete();
                    }
                }, RxSchedulers.TaskpoolScheduler);

                await Signal.Merge(loadDatabase, installChromeDriver, loadUseragent).ToHotTask();

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