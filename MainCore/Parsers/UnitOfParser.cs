using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers
{
    [RegisterAsTransient]
    public class UnitOfParser : IUnitOfParser
    {
        public UnitOfParser(IAccountInfoParser accountInfoParser, IBuildingParser buildingParser, ICompleteImmediatelyParser completeImmediatelyParser, IFarmParser farmParser, IFieldParser fieldParser, IHeroParser heroParser, IInfrastructureParser infrastructureParser, ILoginPageParser loginPageParser, INavigationBarParser navigationBarParser, IQueueBuildingParser queueBuildingParser, IStockBarParser stockBarParser, ITroopPageParser troopPageParser, IVillagePanelParser villagePanelParser, INavigationTabParser navigationTabParser, IUpgradeBuildingParser upgradeBuildingParser, IMarketParser marketParser)
        {
            AccountInfoParser = accountInfoParser;
            BuildingParser = buildingParser;
            CompleteImmediatelyParser = completeImmediatelyParser;
            FarmParser = farmParser;
            FieldParser = fieldParser;
            HeroParser = heroParser;
            InfrastructureParser = infrastructureParser;
            LoginPageParser = loginPageParser;
            NavigationBarParser = navigationBarParser;
            QueueBuildingParser = queueBuildingParser;
            StockBarParser = stockBarParser;
            TroopPageParser = troopPageParser;
            VillagePanelParser = villagePanelParser;
            NavigationTabParser = navigationTabParser;
            UpgradeBuildingParser = upgradeBuildingParser;
            MarketParser = marketParser;
        }

        public IAccountInfoParser AccountInfoParser { get; }
        public IBuildingParser BuildingParser { get; }
        public ICompleteImmediatelyParser CompleteImmediatelyParser { get; }
        public IFarmParser FarmParser { get; }
        public IFieldParser FieldParser { get; }
        public IHeroParser HeroParser { get; }
        public IInfrastructureParser InfrastructureParser { get; }
        public ILoginPageParser LoginPageParser { get; }
        public INavigationBarParser NavigationBarParser { get; }
        public INavigationTabParser NavigationTabParser { get; }
        public IQueueBuildingParser QueueBuildingParser { get; }
        public IStockBarParser StockBarParser { get; }
        public ITroopPageParser TroopPageParser { get; }
        public IVillagePanelParser VillagePanelParser { get; }
        public IUpgradeBuildingParser UpgradeBuildingParser { get; }
        public IMarketParser MarketParser { get; }
    }
}