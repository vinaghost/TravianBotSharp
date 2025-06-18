using MainCore.Enums;

namespace MainCore.Models
{
    public class AttackPlan
    {
        public int X { get; set; }
        public int Y { get; set; }
        public AttackTypeEnums Type { get; set; }
        public int[] Troops { get; set; } = Array.Empty<int>();
    }
}
