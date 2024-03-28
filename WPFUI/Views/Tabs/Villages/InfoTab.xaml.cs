using MainCore.UI.ViewModels.Tabs.Villages;
using ReactiveUI;
using System.Reactive.Disposables;

namespace WPFUI.Views.Tabs.Villages
{
    public class InfoTabBase : ReactiveUserControl<InfoViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for InfoTab.xaml
    /// </summary>
    public partial class InfoTab : InfoTabBase
    {
        public InfoTab()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.SettleAmount, v => v.SettleAmount.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ExpansionSlot, v => v.ExpansionSlot.Text).DisposeWith(d);
            });
        }
    }
}