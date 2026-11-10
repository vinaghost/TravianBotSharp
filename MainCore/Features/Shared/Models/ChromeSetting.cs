namespace MainCore.Features.Shared.Models
{
    public class ChromeSetting
    {
        public required string ProfilePath { get; set; }
        public string? ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public string? ProxyUsername { get; set; }
        public string? ProxyPassword { get; set; }
        public bool IsHeadless { get; set; }
    }
}