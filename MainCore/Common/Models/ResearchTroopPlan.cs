using Humanizer;
using MainCore.Common.Enums;

namespace MainCore.Common.Models
{
    public class ResearchTroopPlan
    {
        public TroopEnums Type { get; set; }

        public override string ToString() => $"Research {Type.Humanize()}";
    }
}