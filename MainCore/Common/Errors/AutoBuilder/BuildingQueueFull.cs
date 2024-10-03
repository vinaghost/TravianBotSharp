namespace MainCore.Common.Errors.AutoBuilder
{
    public class BuildingQueueFull : Error
    {
        protected BuildingQueueFull()
        {
        }

        public static BuildingQueueFull Error => new();
    }
}