using FluentAssertions;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TestProject.Infrasturecture.Persistence
{
    [TestClass]
    public class AppDbContextTest
    {
        private const string _connectionString = "DataSource=TBS-create.db;Cache=Shared";

        [TestMethod]
        public void AppDbContext_Create_ShouldNotThrow()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connectionString);

            using var context = new AppDbContext(optionsBuilder.Options);
            context.Database.EnsureDeleted();
            var createFunc = context.Database.EnsureCreated;
            createFunc.Should().NotThrow();
        }
    }
}