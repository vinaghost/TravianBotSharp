using MainCore.Common.Enums;
using MainCore.UI.Models.Output;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace MainCore.UI.ViewModels.Abstract
{
    public class TroopSelectorViewModel : ViewModelBase
    {
        public ObservableCollection<TroopItem> Items { get; } = new();
        private TroopItem _selectedItem;

        public TroopItem SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }

        public TroopEnums Get()
        {
            return SelectedItem.Troop;
        }
    }
}