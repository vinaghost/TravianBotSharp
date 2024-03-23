using Humanizer;
using MainCore.Common.Enums;

namespace MainCore.Common.Models
{
    public class ResourceBuildPlan
    {
        public int Level { get; set; }
        public ResourcePlanEnums Plan { get; set; }

        public override string ToString() => $"Build {Plan.Humanize()} to level {Level}";
    }
}