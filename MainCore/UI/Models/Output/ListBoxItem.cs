namespace MainCore.UI.Models.Output
{
    public partial class ListBoxItem : ReactiveObject
    {
        public int Id { get; set; }

        [Reactive]
        private string _content;

        [Reactive]
        private SplatColor _color = SplatColor.Black;
    }
}