namespace MainCore.Parsers
{
    public interface IUnitOfParser
    {
        IAccountInfoParser AccountInfoParser { get; }
        IBuildingParser BuildingParser { get; }
        ICompleteImmediatelyParser CompleteImmediatelyParser { get; }
        IFarmParser FarmParser { get; }
        IFieldParser FieldParser { get; }
        IHeroParser HeroParser { get; }
        IInfrastructureParser InfrastructureParser { get; }
        ILoginPageParser LoginPageParser { get; }
        INavigationBarParser NavigationBarParser { get; }
        IQueueBuildingParser QueueBuildingParser { get; }
        IStockBarParser StockBarParser { get; }
        ITroopPageParser TroopPageParser { get; }
        IVillagePanelParser VillagePanelParser { get; }
        INavigationTabParser NavigationTabParser { get; }
        IUpgradeBuildingParser UpgradeBuildingParser { get; }
        IMarketParser MarketParser { get; }
        IOptionPageParser OptionPageParser { get; }
    }
}