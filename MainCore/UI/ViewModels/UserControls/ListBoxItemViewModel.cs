using System.Collections.ObjectModel;

using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;

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

        public ObservableCollection<ListBoxItem> Items { get; } = [];

        public void Load(List<ListBoxItem> inputs, int oldId = -1)
        {
            if (oldId == -1 && SelectedItem is not null)
            {
                oldId = SelectedItem.Id;
            }

            Items.Clear();
            foreach (var input in inputs)
            {
                Items.Add(input);
            }

            if (oldId >= 0 && oldId < Items.Count)
            {
                var item = Items.FirstOrDefault(x => x.Id == oldId);
                SelectedItem = item;
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