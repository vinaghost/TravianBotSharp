namespace MainCore.Services
{
    [RegisterScoped<DataService>]
    public sealed class DataService
    {
        public AccountId AccountId { get; set; }
        public string AccountData { get; set; } = "";
        public bool IsLoggerConfigured { get; set; } = false;
    }
}