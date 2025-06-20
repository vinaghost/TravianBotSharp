using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using System.Collections.ObjectModel;

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

        public void Load(List<ListBoxItem> inputs)
        {
            if (Items.Count == 0)
            {
                foreach (var input in inputs)
                {
                    Items.Add(input);
                }
                return;
            }

            if (inputs.Count == 0)
            {
                Items.Clear();
                return;
            }

            var oldId = SelectedItem?.Id ?? -1;

            for (var i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                var item = Items.FirstOrDefault(x => x.Id == input.Id);

                if (item is not null)
                {
                    item.Content = input.Content;
                    item.Color = input.Color;

                    var index = Items.IndexOf(item);

                    if (index != i)
                        Items.Move(index, i);
                }
                else
                {
                    Items.Insert(i, input);
                }

                if (input.Id == oldId)
                {
                    SelectedIndex = i;
                }
            }

            while (Items.Count > inputs.Count)
            {
                Items.RemoveAt(Items.Count - 1);
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