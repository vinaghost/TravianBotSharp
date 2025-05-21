namespace MainCore.Errors
{
    public class Retry : Error
    {
        private Retry(string message) : base($"{message}. Bot must retry")
        {
        }

        public static Retry NotFound(string name, string type) => new($"Cannot find {type} [{name}] ");

        public static Retry TextboxNotFound(string name) => NotFound(name, "textbox");

        public static Retry ButtonNotFound(string name) => NotFound(name, "button");

        public static Retry ElementNotFound(By by) => new($"Element {by} not found");

        public static Retry ElementNotClickable(By by) => new($"Element {by} not clickable");

        public static Retry OutOfIndexTab(int index, int count) => new($"Found {count} tabs but need tab {index + 1} active");
    }
}