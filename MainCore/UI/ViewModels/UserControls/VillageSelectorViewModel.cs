using DynamicData;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace MainCore.UI.ViewModels.UserControls
{
    public class VillageSelectorViewModel : ViewModelBase
    {
        public ObservableCollection<ComboBoxItem<int>> Items { get; } = new();
        private ComboBoxItem<int> _selectedItem;

        public ComboBoxItem<int> SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }

        public void Set(IList<ComboBoxItem<int>> items)
        {
            Items.Clear();
            Items.AddRange(items);
        }

        public void Set(int currentVillage)
        {
            SelectedItem = Items.FirstOrDefault(x => x.Content == currentVillage);
        }

        public int Get()
        {
            return SelectedItem?.Content ?? 0;
        }
    }
}