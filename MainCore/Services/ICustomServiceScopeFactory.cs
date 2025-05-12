using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Services
{
    public interface ICustomServiceScopeFactory
    {
        IServiceScope CreateScope(AccountId accountId);
    }
}