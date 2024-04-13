using MainCore.Common.Models;
using MainCore.Entities;

namespace MainCore.Services
{
    public interface IAlertService
    {
        List<IncomingAttack> Get(AccountId accountId);
        bool Update(AccountId accountId, List<IncomingAttack> attacks);
    }
}