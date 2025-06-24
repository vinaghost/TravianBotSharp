using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<HeroFarmingViewModel>]
    public class HeroFarmingViewModel : AccountTabViewModelBase
    {
        public ObservableCollection<OasisHeroFarmItem> Oasises { get; } = [];

        protected override async Task Load(AccountId accountId)
        {
            if (Oasises.Count == 0)
            {
                RxApp.MainThreadScheduler.Schedule(() =>
                {
                    Oasises.Add(new OasisHeroFarmItem { X = 1, Y = 2, DateTime = DateTime.Now.AddSeconds(1), Type = "Clay" });
                    Oasises.Add(new OasisHeroFarmItem { X = 3, Y = 4, DateTime = DateTime.Now.AddSeconds(2), Type = "Iron" });
                    Oasises.Add(new OasisHeroFarmItem { X = 5, Y = 6, DateTime = DateTime.Now.AddSeconds(3), Type = "Crop" });
                    Oasises.Add(new OasisHeroFarmItem { X = 7, Y = 8, DateTime = DateTime.Now.AddSeconds(4), Type = "Mixed" });
                });
            }
            await Task.CompletedTask;
        }
    }
}