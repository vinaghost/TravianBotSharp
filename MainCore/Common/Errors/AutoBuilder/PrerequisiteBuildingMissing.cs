namespace MainCore.Common.Errors.AutoBuilder
{
    public class PrerequisiteBuildingMissing(BuildingEnums prerequisiteBuilding, int level)
        : Error("{prerequisiteBuilding} level {level} is missing")
    {
        public void Log(ILogger logger) => logger.Warning(Message, prerequisiteBuilding, level);
    }
}