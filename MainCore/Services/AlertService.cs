using MainCore.Common.Models;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Services
{
    [RegisterAsSingleton]
    public class AlertService : IAlertService
    {
        private readonly Dictionary<VillageId, List<IncomingAttack>> _attacksDict = new();

        public List<IncomingAttack> Get(VillageId villageId)
        {
            return _attacksDict[villageId];
        }

        public bool Update(VillageId villageId, List<IncomingAttack> attacks)
        {
            if (!_attacksDict.ContainsKey(villageId))
            {
                _attacksDict.Add(villageId, attacks);
                return true;
            }

            var source = _attacksDict[villageId]
                .Where(x => x.ArrivalTime < DateTime.Now)
                .ToList();
            source
                .ForEach(x => x.IsNew = false);
            if (IsSame(source, attacks)) return false;

            _attacksDict[villageId] = attacks;
            return true;
        }

        private static bool IsSame(List<IncomingAttack> source, List<IncomingAttack> target)
        {
            if (source.Count != target.Count) return false;

            for (int i = 0; i < source.Count; i++)
            {
                var sourceAttack = source[i];
                var targetAttack = target[i];

                targetAttack.IsNew = !IsSame(sourceAttack, targetAttack);
            }
            return !target.Any(x => x.IsNew);
        }

        private static bool IsSame(IncomingAttack source, IncomingAttack target)
        {
            if (source.VillageName != target.VillageName) return false;
            if (source.WaveCount != target.WaveCount) return false;
            if (source.ArrivalTime != target.ArrivalTime) return false;
            return true;
        }
    }
}