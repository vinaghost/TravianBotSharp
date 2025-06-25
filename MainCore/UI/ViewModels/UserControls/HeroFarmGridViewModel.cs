using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using System.Collections.ObjectModel;

namespace MainCore.UI.ViewModels.UserControls
{
    public partial class HeroFarmGridViewModel : ViewModelBase
    {
        [Reactive]
        private int _selectedIndex;

        [Reactive]
        private HeroFarmItem? _selectedItem;

        [Reactive]
        private bool _isEnable = true;

        public ObservableCollection<HeroFarmItem> Items { get; } = new();

        public void Load(List<HeroFarmItem> inputs)
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
                    item.Set(input);

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

        public HeroFarmItem this[int i]
        {
            get => Items[i];
        }

        public int Count => Items.Count;
    }
}