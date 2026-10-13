using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    [RegisterSingleton<InfoViewModel>]
    public class InfoViewModel : VillageTabViewModelBase
    {
        protected override Task Load(AccountId accountId, VillageId villageId)
        {
            return Task.CompletedTask;
        }
    }
}