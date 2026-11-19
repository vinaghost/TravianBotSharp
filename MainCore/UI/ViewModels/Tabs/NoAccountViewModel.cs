using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<NoAccountViewModel>]
    public class NoAccountViewModel : TabViewModelBase
    {
        public NoAccountViewModel(DialogService dialogService) : base(dialogService)
        {
        }
    }
}