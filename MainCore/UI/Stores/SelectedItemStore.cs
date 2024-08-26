using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.Stores
{
    [RegisterSingleton(Registration = RegistrationStrategy.Self)]
    public class SelectedItemStore : ViewModelBase
    {
        public SelectedItemStore()
        {
            var accountObservable = this.WhenAnyValue(vm => vm.Account);
            accountObservable
                .Select(x => x is not null)
                .ToProperty(this, vm => vm.IsAccountSelected, out _isAccountSelected);
            accountObservable
                .Select(x => x is null)
                .ToProperty(this, vm => vm.IsAccountNotSelected, out _isAccountNotSelected);

            var villageObservable = this.WhenAnyValue(vm => vm.Village);
            villageObservable
                .Select(x => x is not null)
                .ToProperty(this, vm => vm.IsVillageSelected, out _isVillageSelected);
            villageObservable
                .Select(x => x is null)
                .ToProperty(this, vm => vm.IsVillageNotSelected, out _isVillageNotSelected);
        }

        private ListBoxItem _account;

        public ListBoxItem Account
        {
            get => _account;
            set => this.RaiseAndSetIfChanged(ref _account, value);
        }

        private readonly ObservableAsPropertyHelper<bool> _isAccountSelected;
        private readonly ObservableAsPropertyHelper<bool> _isAccountNotSelected;

        public bool IsAccountSelected => _isAccountSelected.Value;
        public bool IsAccountNotSelected => _isAccountNotSelected.Value;

        private ListBoxItem _village;

        public ListBoxItem Village
        {
            get => _village;
            set => this.RaiseAndSetIfChanged(ref _village, value);
        }

        private readonly ObservableAsPropertyHelper<bool> _isVillageSelected;
        private readonly ObservableAsPropertyHelper<bool> _isVillageNotSelected;

        public bool IsVillageSelected => _isVillageSelected.Value;
        public bool IsVillageNotSelected => _isVillageNotSelected.Value;
    }
}