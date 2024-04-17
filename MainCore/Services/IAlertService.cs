using MainCore.Common.Models;
using MainCore.Entities;

namespace MainCore.Services
{
    public interface IAlertService
    {
        List<IncomingAttack> Get(VillageId villageId);

        bool Update(VillageId villageId, List<IncomingAttack> attacks);
    }
}