namespace MainCore.UI.Models.Output
{
    public class ComboBoxItem<T>
    {
        public ComboBoxItem(T value, string content)
        {
            Value = value;
            Content = content;
        }

        public T Value { get; set; }
        public string Content { get; set; }
    }
}