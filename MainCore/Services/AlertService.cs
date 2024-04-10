using MainCore.Common.Models;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Services
{
    [RegisterAsSingleton]
    public class AlertService : IAlertService
    {
        private readonly Dictionary<AccountId, List<IncomingAttack>> _attacksDict = new();

        public List<IncomingAttack> Get(AccountId accountId)
        {
            return _attacksDict[accountId];
        }

        public bool Update(AccountId accountId, List<IncomingAttack> attacks)
        {
            if (!_attacksDict.ContainsKey(accountId))
            {
                _attacksDict.Add(accountId, attacks);
                return true;
            }

            var source = _attacksDict[accountId];
            if (IsSame(source, attacks)) return false;

            _attacksDict[accountId] = attacks;
            return true;
        }

        private static bool IsSame(List<IncomingAttack> source, List<IncomingAttack> target)
        {
            if (source.Count != target.Count) return false;

            foreach (var attack in target)
            {
                var srcAttack = source.FirstOrDefault(x => x.PlayerName == attack.PlayerName);
                if (srcAttack is null) return false;
                if (srcAttack.WaveCount != attack.WaveCount) return false;
            }
            return true;
        }
    }
}