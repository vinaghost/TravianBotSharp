using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.Stores
{
    using ReactiveUI.Primitives;

    [RegisterSingleton<VillageSelectionStateStore>]
    public partial class VillageSelectionStateStore : ViewModelBase
    {
        private readonly Dictionary<int, int> _lastSelectedVillageByAccount = [];
        private IReadOnlyList<ListBoxItem> _availableVillages = [];

        [Reactive]
        private AccountId _currentAccountId = AccountId.Empty;

        [Reactive]
        private VillageId _selectedVillageId = VillageId.Empty;

        [Reactive]
        private bool _hasVillageContext;

        public bool IsVillageContextInvalid => !HasVillageContext;

        public IReadOnlyList<ListBoxItem> AvailableVillages => _availableVillages;

        public void UpdateAvailableVillages(AccountId accountId, IReadOnlyList<ListBoxItem> villages)
        {
            CurrentAccountId = accountId;
            _availableVillages = villages.ToList();
            this.RaisePropertyChanged(nameof(AvailableVillages));
        }

        public VillageId ResolveSelection(AccountId accountId)
        {
            CurrentAccountId = accountId;

            if (_availableVillages.Count == 0)
            {
                InvalidateVillageContext(accountId);
                return VillageId.Empty;
            }

            var preferredVillageId = GetPreferredVillageId(accountId);
            var hasPreferredVillage = preferredVillageId != VillageId.Empty
                                     && _availableVillages.Any(x => x.Id == preferredVillageId.Value);

            var selectedVillageId = hasPreferredVillage
                ? preferredVillageId
                : new VillageId(_availableVillages[0].Id);

            SetSelectedVillage(accountId, selectedVillageId);
            return selectedVillageId;
        }

        public VillageId GetPreferredVillageId(AccountId accountId)
        {
            if (CurrentAccountId == accountId && SelectedVillageId != VillageId.Empty)
            {
                return SelectedVillageId;
            }

            return _lastSelectedVillageByAccount.TryGetValue(accountId.Value, out var villageId)
                ? new VillageId(villageId)
                : VillageId.Empty;
        }

        public void SetSelectedVillage(AccountId accountId, VillageId villageId)
        {
            CurrentAccountId = accountId;
            SelectedVillageId = villageId;
            HasVillageContext = villageId != VillageId.Empty;
            NotifyContextFlagsChanged();

            if (HasVillageContext)
            {
                _lastSelectedVillageByAccount[accountId.Value] = villageId.Value;
            }
        }

        public void InvalidateVillageContext(AccountId accountId)
        {
            CurrentAccountId = accountId;
            SelectedVillageId = VillageId.Empty;
            HasVillageContext = false;
            NotifyContextFlagsChanged();
        }

        private void NotifyContextFlagsChanged()
        {
            this.RaisePropertyChanged(nameof(IsVillageContextInvalid));
        }
    }
}