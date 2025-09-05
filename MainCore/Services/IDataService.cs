namespace MainCore.Services
{
    public interface IDataService
    {
        AccountId AccountId { get; set; }
        string AccountData { get; set; }
        bool IsLoggerConfigured { get; set; }
    }
}
