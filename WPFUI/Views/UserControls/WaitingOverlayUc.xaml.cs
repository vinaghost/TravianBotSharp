using MainCore.UI.ViewModels.UserControls;
using ReactiveUI;
using System.Reactive.Disposables;

namespace WPFUI.Views.UserControls
{
    public class WaitingOverlayUcBase : ReactiveUserControl<WaitingOverlayViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for WaitingOverlayUc.xaml
    /// </summary>
    public partial class WaitingOverlayUc : WaitingOverlayUcBase
    {
        public WaitingOverlayUc()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Shown, v => v.Grid.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Message, v => v.Message.Text).DisposeWith(d);
            });
        }
    }
}