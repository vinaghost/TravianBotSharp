using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<NoVillageViewModel>]
    public class NoVillageViewModel : TabViewModelBase
    {
        public NoVillageViewModel(DialogService dialogService) : base(dialogService)
        {
        }
    }
}