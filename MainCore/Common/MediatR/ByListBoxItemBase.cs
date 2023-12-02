using MainCore.UI.ViewModels.UserControls;

namespace MainCore.Common.MediatR
{
    public class ByListBoxItemBase
    {
        public ByListBoxItemBase(ListBoxItemViewModel items)
        {
            Items = items;
        }

        public ListBoxItemViewModel Items { get; }
    }
}