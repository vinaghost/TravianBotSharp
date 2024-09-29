namespace MainCore.Commands.Abstract
{
    public class QueryBase(IDbContextFactory<AppDbContext> contextFactory)
    {
        protected readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
    }
}