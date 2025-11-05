using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;

namespace WPFUI.Views.UserControls
{
    public class MainLayoutUcBase : ReactiveUserControl<MainLayoutViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for MainLayoutUc.xaml
    /// </summary>
    public partial class MainLayoutUc : MainLayoutUcBase
    {
        public MainLayoutUc()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                // commands
                this.BindCommand(ViewModel, vm => vm.AddAccountCommand, v => v.AddAccountButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.AddAccountsCommand, v => v.AddAccountsButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.LoginCommand, v => v.LoginButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.LogoutCommand, v => v.LogoutButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.DeleteAccountCommand, v => v.DeleteButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.PauseCommand, v => v.PauseButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RestartCommand, v => v.RestartButton).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Version, v => v.Version.Content).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PauseText, v => v.PauseButton.Content).DisposeWith(d);

                // account list
                this.OneWayBind(ViewModel, vm => vm.Accounts.Items, v => v.AccountGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Accounts.SelectedItem, v => v.AccountGrid.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Accounts.IsEnable, v => v.AccountGrid.IsEnabled).DisposeWith(d);

                // tabs
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.NoAccountViewModel, v => v.NoAccount.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.AddAccountViewModel, v => v.AddAccount.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.AddAccountsViewModel, v => v.AddAccounts.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.AccountSettingViewModel, v => v.AccountSetting.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.VillageViewModel, v => v.Village.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.EditAccountViewModel, v => v.EditAccount.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.FarmingViewModel, v => v.Farming.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.DebugViewModel, v => v.Debug.ViewModel).DisposeWith(d);

                // visible
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsNoAccountTabVisible, v => v.NoAccountTab.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsAddAccountTabVisible, v => v.AddAccountTab.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsAddAccountsTabVisible, v => v.AddAccountsTab.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsNormalTabVisible, v => v.AccountSettingTab.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsNormalTabVisible, v => v.VillageTab.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsNormalTabVisible, v => v.EditAccountTab.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsNormalTabVisible, v => v.FarmingTab.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsNormalTabVisible, v => v.DebugTab.Visibility).DisposeWith(d);

                // selected
                this.Bind(ViewModel, vm => vm.AccountTabStore.NoAccountViewModel.IsActive, v => v.NoAccountTab.IsSelected).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountTabStore.AddAccountViewModel.IsActive, v => v.AddAccountTab.IsSelected).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountTabStore.AddAccountsViewModel.IsActive, v => v.AddAccountsTab.IsSelected).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountTabStore.AccountSettingViewModel.IsActive, v => v.AccountSettingTab.IsSelected).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountTabStore.VillageViewModel.IsActive, v => v.VillageTab.IsSelected).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountTabStore.EditAccountViewModel.IsActive, v => v.EditAccountTab.IsSelected).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountTabStore.FarmingViewModel.IsActive, v => v.FarmingTab.IsSelected).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountTabStore.DebugViewModel.IsActive, v => v.DebugTab.IsSelected).DisposeWith(d);
            });
        }
    }
}