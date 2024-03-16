using Humanizer;
using MainCore.Common.Enums;
using MainCore.Common.Extensions;

namespace MainCore.Common.Models
{
    public class TrainTroopPlan
    {
        public TroopEnums Type { get; set; }
        public int Amount { get; set; }
        public bool Great { get; set; }

        public override string ToString()
        {
            var building = Great ? $"Great {Type.GetTrainBuilding().Humanize()}" : Type.GetTrainBuilding().Humanize();
            return $"Train {Amount} {Type.Humanize()} in {building}";
        }
    }
}