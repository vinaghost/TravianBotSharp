namespace MainCore.Services
{
    [RegisterScoped<IDataService, DataService>]
    public sealed class DataService : IDataService
    {
        public AccountId AccountId { get; set; }
        public string AccountData { get; set; } = "";
        public bool IsLoggerConfigured { get; set; } = false;
    }
}