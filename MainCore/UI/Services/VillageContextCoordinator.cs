using MainCore.UI.Models.Output;
using MainCore.UI.Stores;

namespace MainCore.UI.Services
{
    [RegisterSingleton<VillageContextCoordinator>]
    public class VillageContextCoordinator
    {
        private readonly SelectedItemStore _selectedItemStore;
        private readonly VillageSelectionStateStore _villageSelectionStateStore;

        public VillageContextCoordinator(SelectedItemStore selectedItemStore, VillageSelectionStateStore villageSelectionStateStore)
        {
            _selectedItemStore = selectedItemStore;
            _villageSelectionStateStore = villageSelectionStateStore;
        }

        public ListBoxItem? ResolveAndApply(AccountId accountId, List<ListBoxItem> villages)
        {
            _villageSelectionStateStore.UpdateAvailableVillages(accountId, villages);

            var selectedVillageId = _villageSelectionStateStore.ResolveSelection(accountId);
            if (selectedVillageId == VillageId.Empty)
            {
                _selectedItemStore.ClearVillage();
                return null;
            }

            var selectedVillage = villages.FirstOrDefault(x => x.Id == selectedVillageId.Value);
            if (selectedVillage is null)
            {
                _selectedItemStore.ClearVillage();
                return null;
            }

            _selectedItemStore.SetVillage(selectedVillage);
            return selectedVillage;
        }

        public void SetVillageSelection(AccountId accountId, ListBoxItem? village)
        {
            if (village is null)
            {
                _villageSelectionStateStore.InvalidateVillageContext(accountId);
                _selectedItemStore.ClearVillage();
                return;
            }

            var villageId = new VillageId(village.Id);
            _villageSelectionStateStore.SetSelectedVillage(accountId, villageId);
            _selectedItemStore.SetVillage(village);
        }
    }
}