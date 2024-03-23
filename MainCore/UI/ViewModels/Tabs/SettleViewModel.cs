using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class SettleViewModel : AccountTabViewModelBase
    {
        protected override async Task Load(AccountId accountId)
        {
            await Task.CompletedTask;
        }
    }
}