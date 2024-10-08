using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Test.Commands.UI.Tabs
{
    public abstract class Base : IDisposable
    {
        protected readonly IDbContextFactory<AppDbContext> _contextFactory;

        protected Base()
        {
            _contextFactory = new FakeDbContextFactory();

            using var context = _contextFactory.CreateDbContext();
            context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Database.EnsureDeleted();
        }
    }
}