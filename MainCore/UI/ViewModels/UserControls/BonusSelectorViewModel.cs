using MainCore.Common.Enums;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace MainCore.UI.ViewModels.UserControls
{
    public class BonusSelectorViewModel : ViewModelBase
    {
        public BonusSelectorViewModel()
        {
            var bonuses = Enum.GetValues<AllianceBonusEnums>()
               .AsEnumerable()
               .Select(x => new BonusItem(x))
               .ToList();
            Items = new(bonuses);
            SelectedItem = Items[0];
        }

        public void Set(AllianceBonusEnums bonus)
        {
            SelectedItem = Items.Where(x => x.Bonus == bonus).First();
        }

        public AllianceBonusEnums Get()
        {
            return SelectedItem.Bonus;
        }

        public ObservableCollection<BonusItem> Items { get; }
        private BonusItem _selectedItem;

        public BonusItem SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }
    }
}