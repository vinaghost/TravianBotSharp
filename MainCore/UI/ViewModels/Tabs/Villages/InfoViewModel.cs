using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton]
    public class InfoViewModel : VillageTabViewModelBase
    {
        protected override Task Load(VillageId villageId)
        {
            return Task.CompletedTask;
        }
    }
}