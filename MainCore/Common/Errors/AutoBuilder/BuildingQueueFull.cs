namespace MainCore.Common.Errors.AutoBuilder
{
    public class BuildingQueueFull : Error
    {
        protected BuildingQueueFull(bool plusActive, int queueLength) : base("Building queue is full")
        {
            Metadata.Add("IsPlusActive", plusActive);
            Metadata.Add("QueueLength", queueLength);
        }

        public static BuildingQueueFull Error(bool plusActive, int queueLength)
            => new BuildingQueueFull(plusActive, queueLength);
    }
}