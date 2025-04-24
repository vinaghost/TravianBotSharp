namespace MainCore.Services
{
    public interface IDataService
    {
        AccountId AccountId { get; set; }
        IChromeBrowser ChromeBrowser { get; set; }
        ILogger Logger { get; set; }
        VillageId VillageId { get; set; }
    }
}