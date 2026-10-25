using MainCore.UI.ViewModels.UserControls;

using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;

namespace AvaloniaUI.Views.UserControls
{
    public class WaitingOverlayUcBase : ReactiveUserControl<WaitingOverlayViewModel>
    {
    }

    public partial class WaitingOverlayUc : WaitingOverlayUcBase
    {
        public WaitingOverlayUc()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Shown, v => v.Grid.IsVisible).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Message, v => v.Message.Text).DisposeWith(d);
            });
        }
    }
}
