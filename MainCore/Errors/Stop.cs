namespace MainCore.Errors
{
    public class Stop : Error
    {
        private Stop() : base("Bot must stop")
        {
        }

        private Stop(string message) : base($"{message} so stop this account")
        {
        }

        public static Result Error => new Stop();
        public static Result DriverNotReady => Error.WithError("Driver is not ready.");

        public static Stop BuildingJobQueueBroken
            => new("Building job queue is broken");

        public static Stop PrerequisiteBuildingMissing(BuildingEnums prerequisiteBuilding, int level)
            => new($"{prerequisiteBuilding} level {level} is missing from building job queue");
    }
}