using MainCore.UI.ViewModels.Tabs.Villages;

using ReactiveUI.Avalonia;

namespace AvaloniaUI.Views.Tabs.Villages
{
    public class NoVillageTabBase : ReactiveUserControl<NoVillageViewModel>
    {
    }

    public partial class NoVillageTab : NoVillageTabBase
    {
        public NoVillageTab()
        {
            InitializeComponent();
        }
    }
}
