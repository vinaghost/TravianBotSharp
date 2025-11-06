using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;
using System.Reactive.Disposables.Fluent;

namespace WPFUI.Views.Tabs
{
    public partial class AddAccountsTabBase : ReactiveUserControl<AddAccountsViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for AddAccountsTab.xaml
    /// </summary>
    public partial class AddAccountsTab : AddAccountsTabBase
    {
        public AddAccountsTab()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.AddAccountCommand, v => v.AddButton).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.Input, v => v.AccountsInput.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Accounts, v => v.AccountsView.ItemsSource).DisposeWith(d);
            });
        }
    }
}