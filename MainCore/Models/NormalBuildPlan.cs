using Humanizer;

namespace MainCore.Models
{
    public class NormalBuildPlan
    {
        public int Location { get; set; }
        public int Level { get; set; }
        public BuildingEnums Type { get; set; }

        public override string ToString()
        {
            return $"{Type.Humanize()} at slot {Location} to level {Level}";
        }
    }
}