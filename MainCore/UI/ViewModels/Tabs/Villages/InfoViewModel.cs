using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class InfoViewModel : VillageTabViewModelBase
    {
        protected override Task Load(VillageId villageId)
        {
            return Task.CompletedTask;
        }
    }
}