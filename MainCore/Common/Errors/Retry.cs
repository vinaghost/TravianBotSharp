namespace MainCore.Common.Errors
{
    public class Retry : Error
    {
        private Retry(string message) : base($"{message}. Bot must retry")
        {
        }

        public static Retry Exception(Exception exception) => new(exception.Message);

        public static Retry NotFound(string name, string type) => new($"Cannot find {type} [{name}] ");

        public static Retry TextboxNotFound(string name) => NotFound(name, "textbox");

        public static Retry ButtonNotFound(string name) => NotFound(name, "button");

        public static Retry ElementNotFound() => new("Element not found");

        public static Retry ElementNotClickable() => new("Element not clickable");

        public static Retry OutOfIndexTab(int index, int count) => new($"Found {count} tabs but need tab {index} active");
    }
}