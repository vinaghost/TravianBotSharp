using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Services
{
    [RegisterSingleton<ICustomServiceScopeFactory, CustomServiceScopeFactory>]
    public class CustomServiceScopeFactory : ICustomServiceScopeFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CustomServiceScopeFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public IServiceScope CreateScope(AccountId accountId)
        {
            var scrope = _serviceScopeFactory.CreateScope();
            var dataService = scrope.ServiceProvider.GetRequiredService<IDataService>();
            dataService.AccountId = accountId;
            return scrope;
        }
    }
}