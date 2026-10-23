using System.Collections.ObjectModel;

using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.UserControls
{
    public partial class TroopSelectorViewModel : ViewModelBase
    {
        public ObservableCollection<TroopItem> Items { get; } = new();

        [Reactive]
        private TroopItem? _selectedItem;

        public TroopEnums Get()
        {
            return SelectedItem?.Troop ?? TroopEnums.None;
        }

        public void ChangeTribe(BuildingEnums building, TribeEnums tribe)
        {
            Items.Clear();
            Items.Add(new(TroopEnums.None));
            var troops = GetTroops(building, tribe);
            foreach (var troop in troops)
            {
                Items.Add(new TroopItem(troop));
            }

            SelectedItem = Items.FirstOrDefault(x => x.Troop == SelectedItem?.Troop) ?? Items[0];
        }

        public void Set(TroopEnums selectedTroop, BuildingEnums building, TribeEnums tribe)
        {
            Items.Clear();
            Items.Add(new(TroopEnums.None));
            var troops = GetTroops(building, tribe);
            foreach (var troop in troops)
            {
                Items.Add(new TroopItem(troop));
            }
            SelectedItem = Items.FirstOrDefault(x => x.Troop == selectedTroop) ?? Items[0];
        }

        private static List<TroopEnums> GetTroops(BuildingEnums building, TribeEnums tribe)
        {
            return building switch
            {
                BuildingEnums.Barracks => tribe.GetInfantryTroops(),
                BuildingEnums.Stable => tribe.GetCavalryTroops(),
                BuildingEnums.GreatBarracks => tribe.GetInfantryTroops(),
                BuildingEnums.GreatStable => tribe.GetCavalryTroops(),
                BuildingEnums.Workshop => tribe.GetSiegeTroops(),
                _ => new List<TroopEnums>(),
            };
        }
    }
}