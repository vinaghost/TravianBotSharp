using TestProject.Fake;

namespace TestProject.Repositories
{
    public class RepositoryTestBase<TRepository> where TRepository : class
    {
        protected virtual TRepository GetRepository()
        {
            var contextFactory = new FakeDbContextFactory();
            contextFactory.Setup();
            var t = (TRepository)Activator.CreateInstance(typeof(TRepository), contextFactory);
            return t;
        }
    }
}