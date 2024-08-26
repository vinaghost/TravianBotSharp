using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterAsViewModel]
    public class InfoViewModel : VillageTabViewModelBase
    {
        protected override Task Load(VillageId villageId)
        {
            return Task.CompletedTask;
        }
    }
}