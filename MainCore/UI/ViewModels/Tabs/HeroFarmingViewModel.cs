using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<HeroFarmingViewModel>]
    public class HeroFarmingViewModel : AccountTabViewModelBase
    {
        protected override async Task Load(AccountId accountId)
        {
            await Task.CompletedTask;
        }
    }
}