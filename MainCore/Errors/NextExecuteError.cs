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
    }
}