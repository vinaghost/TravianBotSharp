namespace MainCore.Models
{
    public class BrowserSetting
    {
        public required string ProfilePath { get; set; }
        public string? ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public string? ProxyUsername { get; set; }
        public string? ProxyPassword { get; set; }
        public string? UserAgent { get; set; }
        public bool IsHeadless { get; set; }
        public BrowserType BrowserType { get; set; } = BrowserType.Firefox;
    }

    public enum BrowserType
    {
        Chrome,
        Firefox
    }
}
