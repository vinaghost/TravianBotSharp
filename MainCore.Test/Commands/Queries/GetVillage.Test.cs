using Bogus;
using FluentAssertions;
using MainCore.Common.Enums;
using MainCore.Entities;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Test.Commands.Queries
{
    public sealed class GetVillage(DbFixture fixture) : IClassFixture<DbFixture>
    {
        private readonly DbFixture _fixture = fixture;

        [Fact]
        public void All()
        {
            var getVillage = new MainCore.Commands.Queries.GetVillage(_fixture._contextFactory);

            var result = getVillage.All(new AccountId(1));

            result.Count.Should().Be(11);
        }

        [Fact]
        public void Missing()
        {
            var getVillage = new MainCore.Commands.Queries.GetVillage(_fixture._contextFactory);

            var result = getVillage.Missing(new AccountId(1));

            result.Count.Should().Be(8);
        }

        [Fact]
        public void Active()
        {
            var getVillage = new MainCore.Commands.Queries.GetVillage(_fixture._contextFactory);

            var result = getVillage.Active(new AccountId(1));

            result.Should().Be(new VillageId(12));
        }

        [Fact]
        public void Inactive()
        {
            var getVillage = new MainCore.Commands.Queries.GetVillage(_fixture._contextFactory);

            var result = getVillage.Inactive(new AccountId(1));

            result.Count.Should().Be(10);
        }
    }

    public sealed class DbFixture : IDisposable
    {
        public readonly IDbContextFactory<AppDbContext> _contextFactory = new FakeDbContextFactory();

        public DbFixture()
        {
            using var context = _contextFactory.CreateDbContext();
            context.Database.EnsureCreated();

            var accountId = 1;
            var accountFaker = new Faker<Account>()
                .RuleFor(x => x.Id, f => accountId++)
                .RuleFor(x => x.Username, f => f.Internet.ExampleEmail())
                .RuleFor(x => x.Server, f => f.Internet.Url());

            var villageId = 1;
            var villageFaker = new Faker<Village>()
                .RuleFor(x => x.Id, f => villageId++)
                .RuleFor(x => x.AccountId, f => 1)
                .RuleFor(x => x.Name, f => f.Internet.UserName())
                .RuleFor(x => x.IsActive, f => false);

            var buildingId = 1;
            var buildingFaker = new Faker<Building>()
                .RuleFor(x => x.Id, f => buildingId++)
                .RuleFor(x => x.Type, f => BuildingEnums.Site);

            context.AddRange(accountFaker.Generate(10));
            context.AddRange(villageFaker.Generate(10));

            for (var i = 1; i <= 3; i++)
            {
                buildingFaker
                    .RuleFor(x => x.VillageId, f => i);

                context.AddRange(buildingFaker.Generate(40));
            }

            context.Add(new Village
            {
                Id = villageId + 1,
                AccountId = 1,
                Name = "ActiveVillage",
                IsActive = true
            });

            context.SaveChanges();
        }

        public void Dispose()
        {
            using var context = _contextFactory.CreateDbContext();
            context.Database.EnsureDeleted();
        }
    }
}