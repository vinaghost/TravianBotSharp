namespace MainCore.Common.Errors.AutoBuilder
{
    public class PrerequisiteBuildingMissing : Error
    {
        public PrerequisiteBuildingMissing(BuildingEnums prerequisiteBuilding, int level, bool isProgressing)
        {
            PrerequisiteBuilding = prerequisiteBuilding;
            Level = level;
            IsProgressing = isProgressing;
            Message = $"{prerequisiteBuilding} level {level} is missing";
        }

        public BuildingEnums PrerequisiteBuilding { get; }
        public int Level { get; }
        public bool IsProgressing { get; }
    }
}