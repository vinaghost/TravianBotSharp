namespace MainCore.Services
{
    [RegisterScoped<IDataService>]
    public class DataService : IDataService
    {
        public AccountId AccountId { get; set; }
        public VillageId VillageId { get; set; }
        public IChromeBrowser ChromeBrowser { get; set; }
        public ILogger Logger { get; set; }
    }
}