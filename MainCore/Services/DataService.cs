namespace MainCore.Services
{
    [RegisterScoped<DataService>]
    public class DataService
    {
        public AccountId AccountId { get; set; }
        public VillageId VillageId { get; set; }
        public IChromeBrowser ChromeBrowser { get; set; }
        public ILogger Logger { get; set; }
    }
}