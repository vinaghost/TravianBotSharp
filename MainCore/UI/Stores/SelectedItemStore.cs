using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.Stores
{
    [RegisterSingleton<SelectedItemStore>]
    public partial class SelectedItemStore : ViewModelBase
    {
        public SelectedItemStore()
        {
            var accountObservable = this.WhenAnyValue(vm => vm.Account);

            _isAccountSelectedHelper = accountObservable
                .Select(x => x is not null)
                .ToProperty(this, vm => vm.IsAccountSelected);
            _isAccountNotSelectedHelper = accountObservable
                .Select(x => x is null)
                .ToProperty(this, vm => vm.IsAccountNotSelected);

            var villageObservable = this.WhenAnyValue(vm => vm.Village);
            _isVillageSelectedHelper = villageObservable
                .Select(x => x is not null)
                .ToProperty(this, vm => vm.IsVillageSelected);
            _isVillageNotSelectedHelper = villageObservable
                .Select(x => x is null)
                .ToProperty(this, vm => vm.IsVillageNotSelected);
        }

        [Reactive]
        private ListBoxItem? _account;

        [Reactive]
        private ListBoxItem? _village;

        [ObservableAsProperty]
        private bool _isAccountSelected;

        [ObservableAsProperty]
        private bool _isAccountNotSelected;

        [ObservableAsProperty]
        private bool _isVillageSelected;

        [ObservableAsProperty]
        private bool _isVillageNotSelected;
    }
}
