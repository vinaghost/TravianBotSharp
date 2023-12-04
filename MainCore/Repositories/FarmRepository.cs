using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using MainCore.UI.Models.Output;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class FarmRepository : IFarmRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public FarmRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public List<FarmId> GetActive(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            var farmListIds = context.FarmLists
                    .Where(x => x.AccountId == accountId.Value)
                    .Where(x => x.IsActive)
                    .Select(x => x.Id)
                    .AsEnumerable()
                    .Select(x => new FarmId(x))
                    .ToList();
            return farmListIds;
        }

        public int CountActive(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var count = context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.IsActive)
                .Count();
            return count;
        }

        public void ChangeActive(FarmId farmListId)
        {
            using var context = _contextFactory.CreateDbContext();
            context.FarmLists
               .Where(x => x.Id == farmListId.Value)
               .ExecuteUpdate(x => x.SetProperty(x => x.IsActive, x => !x.IsActive));
        }

        public List<ListBoxItem> GetItems(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var items = context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id,
                    Color = x.IsActive ? Color.Green : Color.Red,
                    Content = x.Name,
                })
                .ToList();

            return items;
        }

        public void Update(AccountId accountId, List<FarmDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var farms = context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .ToList();

            var ids = dtos.Select(x => x.Id.Value).ToList();

            var farmDeleted = farms.Where(x => !ids.Contains(x.Id)).ToList();
            var farmInserted = dtos.Where(x => !farms.Any(v => v.Id == x.Id.Value)).ToList();
            var farmUpdated = farms.Where(x => ids.Contains(x.Id)).ToList();

            farmDeleted.ForEach(x => context.Remove(x));
            farmInserted.ForEach(x => context.Add(x.ToEntity(accountId)));

            foreach (var farm in farmUpdated)
            {
                var dto = dtos.FirstOrDefault(x => x.Id.Value == farm.Id);
                dto.To(farm);
                context.Update(farm);
            }

            context.SaveChanges();
        }
    }
}