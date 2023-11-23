using MainCore.Common.Enums;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace MainCore.UI.ViewModels.UserControls
{
    public class TribeSelectorViewModel : ViewModelBase
    {
        public TribeSelectorViewModel()
        {
            var tribes = Enum.GetValues<TribeEnums>()
                .AsEnumerable()
                .Where(x => x != TribeEnums.Nature)
                .Where(x => x != TribeEnums.Natars)
                .Select(x => new TribeItem(x))
                .ToList();
            Items = new(tribes);
            SelectedItem = Items[0];
        }

        public void Set(TribeEnums tribe)
        {
            SelectedItem = Items.Where(x => x.Tribe == tribe).First();
        }

        public TribeEnums Get()
        {
            return SelectedItem.Tribe;
        }

        public ObservableCollection<TribeItem> Items { get; }
        private TribeItem _selectedItem;

        public TribeItem SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }
    }
}