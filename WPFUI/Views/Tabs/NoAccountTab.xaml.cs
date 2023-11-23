using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;

namespace WPFUI.Views.Tabs
{
    public class NoAccountTabBase : ReactiveUserControl<NoAccountViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for NoAccountTab.xaml
    /// </summary>
    public partial class NoAccountTab : NoAccountTabBase
    {
        public NoAccountTab()
        {
            InitializeComponent();
        }
    }
}