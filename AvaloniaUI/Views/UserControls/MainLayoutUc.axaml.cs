using MainCore.UI.ViewModels.UserControls;

using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;

namespace AvaloniaUI.Views.UserControls
{
    public class MainLayoutUcBase : ReactiveUserControl<MainLayoutViewModel>
    {
    }

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
                this.BindCommand(ViewModel, vm => vm.SetDefaultSettingsForNewAccountCommand, v => v.DefaultSettingsForNewAccountButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SetDefaultSettingsForNewVillageCommand, v => v.DefaultSettingsForNewVillageButton).DisposeWith(d);

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
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsNoAccountTabVisible, v => v.NoAccountTab.IsVisible).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsAddAccountTabVisible, v => v.AddAccountTab.IsVisible).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsAddAccountsTabVisible, v => v.AddAccountsTab.IsVisible).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsNormalTabVisible, v => v.AccountSettingTab.IsVisible).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsNormalTabVisible, v => v.VillageTab.IsVisible).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsNormalTabVisible, v => v.EditAccountTab.IsVisible).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsNormalTabVisible, v => v.FarmingTab.IsVisible).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountTabStore.IsNormalTabVisible, v => v.DebugTab.IsVisible).DisposeWith(d);

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
