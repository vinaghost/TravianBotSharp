using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;
using System.Reactive.Disposables;

namespace WPFUI.Views.Tabs
{
    public class DebugTagBase : ReactiveUserControl<DebugViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for DebugTag.xaml
    /// </summary>
    public partial class DebugTag : DebugTagBase
    {
        public DebugTag()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Logs, v => v.LogView.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Tasks, v => v.TaskView.ItemsSource).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.LeftCommand, v => v.ReportButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RightCommand, v => v.LogButton).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.EndpointAddress, v => v.DevToolsEndpointAddress.Text).DisposeWith(d);
            });
        }
    }
}
