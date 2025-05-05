namespace MainCore.Models
{
    public struct PrerequisiteBuilding
    {
        public PrerequisiteBuilding(BuildingEnums type, int level)
        {
            Type = type;
            Level = level;
        }

        public BuildingEnums Type { get; set; }
        public int Level { get; set; }
    }
}