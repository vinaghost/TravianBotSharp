namespace MainCore.Errors.AutoBuilder
{
    public class JobError : Error
    {
        private JobError(string message) : base(message)
        {
        }

        public static JobError JobNotAvailable(string type)
            => new($"{type} job is not available");

        public static JobError PrerequisiteBuildingMissing(BuildingEnums prerequisiteBuilding, int level)
            => new($"{prerequisiteBuilding} level {level} is missing");

        public static JobError BuildingQueueFull
            => new("Building queue is full");
    }
}