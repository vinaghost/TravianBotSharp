using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;
using System.Reactive.Disposables;

namespace WPFUI.Views.Tabs
{
    public class AlertTabBase : ReactiveUserControl<AlertViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for DebugTag.xaml
    /// </summary>
    public partial class AlertTab : AlertTabBase
    {
        public AlertTab()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.Test, v => v.TestButton).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.DiscordEnable, v => v.DiscordEnable.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.DiscordWebhookUrl, v => v.DiscordWebhookUrl.Text).DisposeWith(d);
            });
        }
    }
}