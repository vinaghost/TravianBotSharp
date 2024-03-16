using MainCore.Common.Enums;

namespace MainCore.Common.Models
{
    public class TrainTroopPlan
    {
        public TroopEnums Type { get; set; }
        public int Amount { get; set; }
        public bool Great { get; set; }
    }
}