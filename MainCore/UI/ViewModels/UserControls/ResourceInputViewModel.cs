using MainCore.UI.ViewModels.Abstract;

namespace MainCore.UI.ViewModels.UserControls
{
    public partial class ResourceInputViewModel : ViewModelBase
    {
        public (int, int, int, int) Get()
        {
            return (Wood, Clay, Iron, Crop);
        }

        public void Set(int wood, int clay, int iron, int crop)
        {
            Wood = wood;
            Clay = clay;
            Iron = iron;
            Crop = crop;
        }

        [Reactive]
        private int _wood;

        [Reactive]
        private int _clay;

        [Reactive]
        private int _iron;

        [Reactive]
        private int _crop;
    }
}
