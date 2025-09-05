using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using System.Collections.ObjectModel;

namespace MainCore.UI.ViewModels.UserControls
{
    public partial class TribeSelectorViewModel : ViewModelBase
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
            SelectedItem = Items.First(x => x.Tribe == tribe);
        }

        public TribeEnums Get()
        {
            return SelectedItem.Tribe;
        }

        public ObservableCollection<TribeItem> Items { get; }

        [Reactive]
        private TribeItem _selectedItem;
    }
}
