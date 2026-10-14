using MainCore.UI.ViewModels.Tabs;

using ReactiveUI.Avalonia;

namespace AvaloniaUI.Views.Tabs
{
    public class NoAccountTabBase : ReactiveUserControl<NoAccountViewModel>
    {
    }

    public partial class NoAccountTab : NoAccountTabBase
    {
        public NoAccountTab()
        {
            InitializeComponent();
        }
    }
}
