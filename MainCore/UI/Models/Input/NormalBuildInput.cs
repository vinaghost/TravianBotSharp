using DynamicData;
using Humanizer;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace MainCore.UI.Models.Input
{
    public class NormalBuildInput : ViewModelBase
    {
        public NormalBuildInput()
        {
            this.WhenAnyValue(vm => vm.SelectedBuilding)
                .WhereNotNull()
                .Subscribe((x) => Level = x.Content.GetMaxLevel());
        }

        public void Set(List<BuildingEnums> buildings, int level = -1)
        {
            Buildings.Clear();
            var comboboxItems = buildings.Select(x => new ComboBoxItem<BuildingEnums>(x, x.Humanize())).ToList();
            Buildings.AddRange(comboboxItems);

            if (comboboxItems.Count > 0)
            {
                SelectedBuilding = Buildings[0];
            }
            else
            {
                SelectedBuilding = null;
            }
            if (level != -1)
            {
                Level = level;
            }
        }

        public (BuildingEnums, int) Get()
        {
            if (SelectedBuilding is null)
            {
                return (BuildingEnums.Site, -1);
            }
            return (SelectedBuilding.Content, Level);
        }

        public void Clear()
        {
            Buildings.Clear();
            SelectedBuilding = null;
            Level = 0;
        }

        public ObservableCollection<ComboBoxItem<BuildingEnums>> Buildings { get; set; } = new();
        private ComboBoxItem<BuildingEnums> _selectedBuilding;

        public ComboBoxItem<BuildingEnums> SelectedBuilding
        {
            get => _selectedBuilding;
            set => this.RaiseAndSetIfChanged(ref _selectedBuilding, value);
        }

        private int _level;

        public int Level
        {
            get => _level;
            set => this.RaiseAndSetIfChanged(ref _level, value);
        }
    }
}