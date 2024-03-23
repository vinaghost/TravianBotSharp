using MainCore.UI.ViewModels.Tabs;
using ReactiveUI;

namespace WPFUI.Views.Tabs
{
    public class SettleTabBase : ReactiveUserControl<SettleViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for SettleTab.xaml
    /// </summary>
    public partial class SettleTab : SettleTabBase
    {
        public SettleTab()
        {
            InitializeComponent();
        }
    }
}