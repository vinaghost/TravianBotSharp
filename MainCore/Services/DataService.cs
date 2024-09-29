namespace MainCore.Services
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class DataService
    {
        public AccountId AccountId { get; set; }
        public VillageId VillageId { get; set; }
        public IChromeBrowser ChromeBrowser { get; set; }
        public ILogger Logger { get; set; }
    }
}