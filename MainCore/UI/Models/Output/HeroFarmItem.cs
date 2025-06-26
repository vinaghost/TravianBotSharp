namespace MainCore.UI.Models.Output
{
    public partial class HeroFarmItem : ReactiveObject
    {
        public int Id { get; set; }

        [Reactive]
        private int _x;

        [Reactive]
        private int _y;

        [Reactive]
        private string _oasisType = "Unknown";

        [Reactive]
        private string _animal = "";

        [Reactive]
        private int _resource;

        [Reactive]
        private DateTime _lastSend = DateTime.MinValue;

        public void Set(HeroFarmItem input)
        {
            Id = input.Id;
            X = input.X;
            Y = input.Y;
            Animal = input.Animal;
            Resource = input.Resource;
            LastSend = input.LastSend;
            OasisType = input.OasisType;
        }
    }
}