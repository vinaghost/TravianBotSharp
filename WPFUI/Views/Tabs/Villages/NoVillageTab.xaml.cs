using MainCore.UI.ViewModels.Tabs.Villages;
using ReactiveUI;

namespace WPFUI.Views.Tabs.Villages
{
    public class NoVillageTabBase : ReactiveUserControl<NoVillageViewModel>
    {
    }

    /// <summary>
    /// Interaction logic for NoVillageTAb.xaml
    /// </summary>
    public partial class NoVillageTab : NoVillageTabBase
    {
        public NoVillageTab()
        {
            InitializeComponent();
        }
    }
}