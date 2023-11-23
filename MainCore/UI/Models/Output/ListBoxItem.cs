using ReactiveUI;
using System.Drawing;

namespace MainCore.UI.Models.Output
{
    public class ListBoxItem : ReactiveObject
    {
        public int Id { get; set; }
        private string _content;

        public string Content
        {
            get => _content;
            set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        private Color _color = Color.Black;

        public Color Color
        {
            get => _color;
            set => this.RaiseAndSetIfChanged(ref _color, value);
        }
    }
}