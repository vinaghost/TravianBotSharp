using MainCore.UI.ViewModels.Tabs.Villages;
using ReactiveUI;

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
        }
    }
}