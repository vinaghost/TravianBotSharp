using MainCore.Common.Enums;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class QueueBuildingRepository : IQueueBuildingRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public QueueBuildingRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public QueueBuilding GetFirst(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .OrderBy(x => x.Position)
                .FirstOrDefault();
            return queueBuilding;
        }

        public void Clean(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var now = DateTime.Now;
            context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Where(x => x.CompleteTime < now)
                .ExecuteUpdate(x => x.SetProperty(x => x.Type, BuildingEnums.Site));
        }

        public int Count(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var count = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site)
                .Count();
            return count;
        }

        public void Update(VillageId villageId, List<BuildingDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var queueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type != BuildingEnums.Site);

            if (dtos.Count == 1)
            {
                var building = dtos[0];
                queueBuildings = queueBuildings
                    .Where(x => x.Type == building.Type);

                var list = queueBuildings.ToList();
                foreach (var item in list)
                {
                    item.Location = building.Location;
                }
                context.UpdateRange(list);
            }
            else if (dtos.Count == 2)
            {
                foreach (var dto in dtos)
                {
                    var list = queueBuildings.ToList();
                    var queueBuilding = list.FirstOrDefault(x => x.Type == dto.Type);
                    queueBuilding.Location = dto.Location;
                    context.Update(queueBuilding);
                }
            }
            context.SaveChanges();
        }

        public void Update(VillageId villageId, List<QueueBuildingDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();

            context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .ExecuteDelete();

            var entities = new List<QueueBuilding>();

            foreach (var dto in dtos)
            {
                var queueBuilding = dto.ToEntity(villageId);
                entities.Add(queueBuilding);
            }

            context.AddRange(entities);
            context.SaveChanges();
        }
    }
}