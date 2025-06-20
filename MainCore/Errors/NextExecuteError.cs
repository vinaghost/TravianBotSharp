namespace MainCore.Errors
{
    public class NextExecuteError : Error
    {
        public DateTime NextExecute { get; set; }

        private NextExecuteError(string message) : base(message)
        {
        }

        public static NextExecuteError ConstructionQueueFull(DateTime nextExecute)
           => new("Construction queue is full") { NextExecute = nextExecute };

        public static NextExecuteError PrerequisiteBuildingInQueue(BuildingEnums prerequisiteBuilding, int level, DateTime completeTime)
           => new($"{prerequisiteBuilding} level {level} is in queue") { NextExecute = completeTime };
    }
}