namespace MainCore.Errors
{
    public class Skip : Error
    {
        private Skip() : base("Skip this task")
        {
        }

        private Skip(string message) : base($"{message} so skip this task ")
        {
        }

        public static Result Error => new Skip();

        public static Skip BuildingJobQueueEmpty
            => new("Building job queue is empty");
    }
}