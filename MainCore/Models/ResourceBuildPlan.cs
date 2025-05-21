using Humanizer;

namespace MainCore.Models
{
    public class ResourceBuildPlan
    {
        public int Level { get; set; }
        public ResourcePlanEnums Plan { get; set; }

        public override string ToString()
        {
            return $"{Plan.Humanize()} to level {Level}";
        }
    }
}