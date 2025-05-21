using MainCore.Entities;
using MainCore.Infrasturecture.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Test
{
    public class FakeDbContextFactory
    {
        private const string _connectionString = "DataSource=:memory:";

        public AppDbContext CreateDbContext(bool setup = false)
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .EnableSensitiveDataLogging()
                .UseSqlite(connection)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            if (setup)
            {
                SetupDatabase(context);
            }
            return context;
        }

        public void SetupDatabase(AppDbContext context)
        {
            context.Add(new Account
            {
                Id = 1,
                Username = "TestAccount",
                Server = "https://test.server",
                Villages = [
                    new Village
                    {
                        Id = 1,
                        Name = "TestVillage",
                    }
                ]
            });

            context.SaveChanges();
        }
    }
}