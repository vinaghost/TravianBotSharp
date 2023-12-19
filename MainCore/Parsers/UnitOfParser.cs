using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Parsers
{
    [RegisterAsTransient(withoutInterface: true)]
    public record UnitOfParser(IAccountInfoParser AccountInfoParser,
                               IBuildingParser BuildingParser,
                               ICompleteImmediatelyParser CompleteImmediatelyParser,
                               IFarmParser FarmParser,
                               IFieldParser FieldParser,
                               IHeroParser HeroParser,
                               IInfrastructureParser InfrastructureParser,
                               ILoginPageParser LoginPageParser,
                               INavigationBarParser NavigationBarParser,
                               INavigationTabParser NavigationTabParser,
                               IQueueBuildingParser QueueBuildingParser,
                               IStockBarParser StockBarParser,
                               ITroopPageParser TroopPageParser,
                               IVillagePanelParser VillagePanelParser,
                               IUpgradeBuildingParser UpgradeBuildingParser,
                               IMarketParser MarketParser,
                               IQuestParser QuestParser,
                               IOptionPageParser OptionPageParser);
}