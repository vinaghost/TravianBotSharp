namespace MainCore.Services
{
    public interface IDataService
    {
        AccountId AccountId { get; set; }
        bool IsLoggerConfigured { get; set; }
    }
}