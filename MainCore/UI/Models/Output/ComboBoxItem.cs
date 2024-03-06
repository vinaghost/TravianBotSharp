namespace MainCore.UI.Models.Output
{
    public class ComboBoxItem<T>
    {
        public ComboBoxItem(T content, string name)
        {
            Content = content;
            Name = name;
        }

        public T Content { get; set; }
        public string Name { get; set; }
    }
}