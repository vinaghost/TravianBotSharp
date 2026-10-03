using Humanizer;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using System.Collections.ObjectModel;

namespace MainCore.UI.Models.Input
{
    using ReactiveUI.Primitives;

    public partial class NormalBuildInput : ViewModelBase
    {
        public NormalBuildInput()
        {
            this.WhenAnyValue(vm => vm.SelectedBuilding)
                .WhereNotNull()
                .Select(x => x.Content)
                .Subscribe((x) => Level = x.GetMaxLevel());
        }

        public void Set(List<BuildingEnums> buildings, int level = -1)
        {
            Buildings.Clear();
            var comboboxItems = buildings.Select(x => new ComboBoxItem<BuildingEnums>(x, x.Humanize())).ToList();
            foreach (var item in comboboxItems)
            {
                Buildings.Add(item);
            }

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

        [Reactive]
        private ComboBoxItem<BuildingEnums>? _selectedBuilding;

        [Reactive]
        private int _level;
    }
}