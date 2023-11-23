using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;

namespace MainCore.UI.ViewModels.UserControls
{
    public class ResourceInputViewModel : ViewModelBase
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

        private int _wood;

        public int Wood
        {
            get => _wood;
            set => this.RaiseAndSetIfChanged(ref _wood, value);
        }

        private int _clay;

        public int Clay
        {
            get => _clay;
            set => this.RaiseAndSetIfChanged(ref _clay, value);
        }

        private int _iron;

        public int Iron
        {
            get => _iron;
            set => this.RaiseAndSetIfChanged(ref _iron, value);
        }

        private int _crop;

        public int Crop
        {
            get => _crop;
            set => this.RaiseAndSetIfChanged(ref _crop, value);
        }
    }
}