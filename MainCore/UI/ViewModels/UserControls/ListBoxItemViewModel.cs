using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using System.Collections.ObjectModel;
using System.Linq;

namespace MainCore.UI.ViewModels.UserControls
{
    public partial class ListBoxItemViewModel : ViewModelBase
    {
        [Reactive]
        private int _selectedIndex;

        [Reactive]
        private ListBoxItem? _selectedItem;

        [Reactive]
        private bool _isEnable = true;

        public ObservableCollection<ListBoxItem> Items { get; } = new();

        public bool IsSelected => SelectedItem is not null;
        public int SelectedItemId => SelectedItem?.Id ?? -1;

        public void Load(IEnumerable<ListBoxItem> items)
        {
            var oldId = SelectedItemId;
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            if (Items.Count > 0)
            {
                var matched = Items.FirstOrDefault(x => x.Id == oldId);
                if (matched is not null)
                {
                    SelectedItem = matched;
                }
                else
                {
                    SelectedItem = Items[0];
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
    }
}