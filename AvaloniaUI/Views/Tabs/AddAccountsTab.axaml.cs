using MainCore.UI.ViewModels.Tabs;

using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;

namespace AvaloniaUI.Views.Tabs
{
    public class AddAccountsTabBase : ReactiveUserControl<AddAccountsViewModel>
    {
    }

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
