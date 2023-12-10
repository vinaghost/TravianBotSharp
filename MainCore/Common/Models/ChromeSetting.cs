namespace MainCore.Common.Models
{
    public class ChromeSetting
    {
        public string ProfilePath { get; set; }
        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
        public string UserAgent { get; set; }
        public bool IsHeadless { get; set; }
    }
}