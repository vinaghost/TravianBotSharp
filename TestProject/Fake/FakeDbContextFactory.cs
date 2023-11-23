using MainCore.Entities;
using MainCore.Infrasturecture.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TestProject.Fake
{
    public class FakeDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptionsBuilder<AppDbContext> _options;

        public FakeDbContextFactory()
        {
            var keepAliveConnection = new SqliteConnection("DataSource=:memory:");
            keepAliveConnection.Open();

            _options = new DbContextOptionsBuilder<AppDbContext>()
                .EnableSensitiveDataLogging()
                .UseSqlite(keepAliveConnection);
        }

        public void Setup()
        {
            using var context = new AppDbContext(_options.Options);
            Create(context);
            SeedData(context);
        }

        public AppDbContext CreateDbContext()
        {
            var context = new AppDbContext(_options.Options);
            return context;
        }

        public static void Create(AppDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        public static void SeedData(AppDbContext context)
        {
            context.Add(new Account()
            {
                Username = "test_account",
                Server = "https://thisisatestserver.com",
                Villages = new List<Village>()
                {
                    new Village()
                    {
                        Name = "test_village",
                    },
                },
            });

            context.SaveChanges();
        }
    }
}