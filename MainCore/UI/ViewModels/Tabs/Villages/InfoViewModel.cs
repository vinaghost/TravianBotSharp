using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<InfoViewModel>]
    public class InfoViewModel : VillageTabViewModelBase
    {
        public InfoViewModel(DialogService dialogService, SelectedItemStore selectedItemStore) : base(dialogService, selectedItemStore)
        {
        }

        protected override Task Load(VillageId villageId)
        {
            return Task.CompletedTask;
        }
    }
}