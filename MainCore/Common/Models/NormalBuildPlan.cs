using Humanizer;
using MainCore.Common.Enums;

namespace MainCore.Common.Models
{
    public class NormalBuildPlan
    {
        public int Location { get; set; }
        public int Level { get; set; }
        public BuildingEnums Type { get; set; }

        public override string ToString() => $"Build {Type.Humanize()} to level {Level} at location {Location}";
    }
}