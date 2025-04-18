using DynamicData;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;

namespace MainCore.UI.ViewModels.UserControls
{
    public partial class TroopSelectorViewModel : ViewModelBase
    {
        public ObservableCollection<TroopItem> Items { get; } = new();

        [Reactive]
        private TroopItem _selectedItem;

        public TroopEnums Get()
        {
            return SelectedItem.Troop;
        }

        public void ChangeTribe(BuildingEnums building, TribeEnums tribe)
        {
            Items.Clear();
            Items.Add(new(TroopEnums.None));
            var troops = GetTroops(building, tribe);
            Items.AddRange(troops.Select(x => new TroopItem(x)));

            SelectedItem = Items.FirstOrDefault(x => x.Troop == SelectedItem?.Troop) ?? Items[0];
        }

        public void Set(TroopEnums selectedTroop, BuildingEnums building, TribeEnums tribe)
        {
            Items.Clear();
            Items.Add(new(TroopEnums.None));
            var troops = GetTroops(building, tribe);
            Items.AddRange(troops.Select(x => new TroopItem(x)));
            SelectedItem = Items.FirstOrDefault(x => x.Troop == selectedTroop) ?? Items[0];
        }

        private static List<TroopEnums> GetTroops(BuildingEnums building, TribeEnums tribe)
        {
            return building switch
            {
                BuildingEnums.Barracks => GetInfantryTroops(tribe),
                BuildingEnums.Stable => GetCavalryTroops(tribe),
                BuildingEnums.GreatBarracks => GetInfantryTroops(tribe),
                BuildingEnums.GreatStable => GetCavalryTroops(tribe),
                BuildingEnums.Workshop => GetSiegeTroops(tribe),
                _ => new List<TroopEnums>(),
            };
        }

        private static List<TroopEnums> GetInfantryTroops(TribeEnums tribe)
        {
            return tribe switch
            {
                TribeEnums.Romans => new()
                {
                    TroopEnums.Legionnaire,
                    TroopEnums.Praetorian,
                    TroopEnums.Imperian,
                },
                TribeEnums.Teutons => new()
                {
                    TroopEnums.Clubswinger,
                    TroopEnums.Spearman,
                    TroopEnums.Axeman,
                    TroopEnums.Scout,
                },
                TribeEnums.Gauls => new()
                {
                   TroopEnums.Phalanx,
                    TroopEnums.Swordsman,
                },
                TribeEnums.Nature => new(),
                TribeEnums.Natars => new(),
                TribeEnums.Egyptians => new()
                {
                    TroopEnums.SlaveMilitia,
                    TroopEnums.AshWarden,
                    TroopEnums.KhopeshWarrior,
                },
                TribeEnums.Huns => new()
                {
                    TroopEnums.Mercenary,
                    TroopEnums.Bowman,
                },
                _ => new(),
            };
        }

        private static List<TroopEnums> GetCavalryTroops(TribeEnums tribe)
        {
            return tribe switch
            {
                TribeEnums.Romans => new()
                {
                    TroopEnums.EquitesLegati,
                    TroopEnums.EquitesImperatoris,
                    TroopEnums.EquitesCaesaris,
                },
                TribeEnums.Teutons => new()
                {
                    TroopEnums.Paladin,
                    TroopEnums.TeutonicKnight,
                },
                TribeEnums.Gauls => new()
                {
                    TroopEnums.Pathfinder,
                    TroopEnums.TheutatesThunder,
                    TroopEnums.Druidrider,
                    TroopEnums.Haeduan,
                },
                TribeEnums.Nature => new(),
                TribeEnums.Natars => new(),
                TribeEnums.Egyptians => new()
                {
                    TroopEnums.SopduExplorer,
                    TroopEnums.AnhurGuard,
                    TroopEnums.ReshephChariot,
                },
                TribeEnums.Huns => new()
                {
                    TroopEnums.Spotter,
                    TroopEnums.SteppeRider,
                    TroopEnums.Marksman,
                    TroopEnums.Marauder,
                },
                _ => new(),
            };
        }

        private static List<TroopEnums> GetSiegeTroops(TribeEnums tribe)
        {
            return tribe switch
            {
                TribeEnums.Romans => new()
                {
                    TroopEnums.RomanRam,
                    TroopEnums.RomanCatapult,
                },
                TribeEnums.Teutons => new()
                {
                    TroopEnums.TeutonRam,
                    TroopEnums.TeutonCatapult,
                },
                TribeEnums.Gauls => new()
                {
                    TroopEnums.GaulRam,
                    TroopEnums.GaulCatapult,
                },
                TribeEnums.Nature => new(),
                TribeEnums.Natars => new(),
                TribeEnums.Egyptians => new()
                {
                    TroopEnums.EgyptianRam,
                    TroopEnums.EgyptianCatapult
                },
                TribeEnums.Huns => new()
                {
                    TroopEnums.HunRam,
                    TroopEnums.HunCatapult,
                },
                _ => new(),
            };
        }
    }
}