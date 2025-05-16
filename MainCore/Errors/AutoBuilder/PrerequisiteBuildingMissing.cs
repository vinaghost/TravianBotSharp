namespace MainCore.Errors.AutoBuilder
{
    public class PrerequisiteBuildingMissing(BuildingEnums prerequisiteBuilding, int level)
        : Error($"{prerequisiteBuilding} level {level} is missing")
    {
    }
}