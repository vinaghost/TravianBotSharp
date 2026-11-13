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

        public void Load(List<ListBoxItem> inputs)
        {
            var oldId = SelectedItem?.Id ?? 0;

            Items.Clear();
            foreach (var input in inputs)
            {
                Items.Add(input);
            }

            if (oldId > 0)
            {
                var item = Items.FirstOrDefault(x => x.Id == oldId);
                SelectedItem = item;
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