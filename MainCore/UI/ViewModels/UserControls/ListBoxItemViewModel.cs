using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace MainCore.UI.ViewModels.UserControls
{
    public class ListBoxItemViewModel : ViewModelBase
    {
        private int _selectedIndex;
        private ListBoxItem _selectedItem;

        public ObservableCollection<ListBoxItem> Items { get; } = new();

        public bool IsSelected => SelectedItem is not null;
        public int SelectedItemId => SelectedItem?.Id ?? -1;

        public ListBoxItem SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
        }

        public void Load(IEnumerable<ListBoxItem> items)
        {
            var oldIndex = SelectedIndex;
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            if (Items.Count > 0)
            {
                if (oldIndex == -1)
                {
                    SelectedItem = Items[0];
                }
                else if (oldIndex < Items.Count)
                {
                    SelectedItem = Items[oldIndex];
                }
                else
                {
                    SelectedItem = Items[^1];
                }
            }
            else
            {
                SelectedItem = null;
            }
        }

        public ListBoxItem this[int i]
        {
            get => Items[i];
            set => Items[i] = value;
        }

        public int Count => Items.Count;

        private bool _isEnable = true;

        public bool IsEnable
        {
            get => _isEnable;
            set => this.RaiseAndSetIfChanged(ref _isEnable, value);
        }
    }
}