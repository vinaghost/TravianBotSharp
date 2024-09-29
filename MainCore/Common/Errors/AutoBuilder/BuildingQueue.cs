namespace MainCore.Common.Errors.AutoBuilder
{
    public class BuildingQueueFull : Error
    {
        private BuildingQueueFull(string message) : base(message)
        {
        }

        public static BuildingQueueFull Error => new("Amount of currently building is equal with maximum building can build in same time");
    }
}