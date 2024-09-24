using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Test
{
    public class FakeDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private const string _connectionString = "DataSource=TBS.Test.db;Cache=Shared";

        public AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .EnableSensitiveDataLogging()
                .UseSqlite(_connectionString)
                .Options;
            var context = new AppDbContext(options);
            return context;
        }
    }
}