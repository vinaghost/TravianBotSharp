namespace MainCore.Errors
{
    public class UpgradeBuildingError : Error
    {
        private UpgradeBuildingError(string message) : base(message)
        {
        }

        public static UpgradeBuildingError BuildingJobQueueEmpty
            => new("Building job queue is empty");

        public static UpgradeBuildingError BuildingJobQueueBroken
            => new("Building job queue is broken. No building in construct but cannot choose job");

        public static UpgradeBuildingError JobNotAvailable(string type)
            => new($"{type} job is not available");

        public static UpgradeBuildingError PrerequisiteBuildingMissing(BuildingEnums prerequisiteBuilding, int level)
            => new($"{prerequisiteBuilding} level {level} is missing");
    }
}
