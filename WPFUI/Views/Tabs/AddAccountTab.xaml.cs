using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;

namespace WPFUI.Views.Tabs
{
    public class AddAccountTabBase : ReactiveUserControl<AddAccountViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for AddAccountTab.xaml
    /// </summary>
    public partial class AddAccountTab : AddAccountTabBase
    {
        public AddAccountTab()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.AddAccessCommand, v => v.AddAccessButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.EditAccessCommand, v => v.EditAccessButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.DeleteAccessCommand, v => v.DeleteAccessButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.AddAccountCommand, v => v.AddAccountButton).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.AccountInput.Username, v => v.NicknameTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccountInput.Server, v => v.ServerTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AccountInput.Accesses, v => v.ProxiesDataGrid.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedAccess, v => v.ProxiesDataGrid.SelectedItem).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.AccessInput.Username, v => v.UsernameTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccessInput.Password, v => v.PasswordTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccessInput.ProxyHost, v => v.ProxyHostTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccessInput.ProxyPort, v => v.ProxyPortTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccessInput.ProxyUsername, v => v.ProxyUsernameTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AccessInput.ProxyPassword, v => v.ProxyPasswordTextBox.Text).DisposeWith(d);
            });
        }
    }
}