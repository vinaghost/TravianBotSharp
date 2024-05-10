namespace MainCore.Common.Errors.AutoBuilder
{
    public class BuildingQueue : Error
    {
        private BuildingQueue(string message) : base(message)
        {
        }

        public static BuildingQueue Full => new("Amount of currently building is equal with maximum building can build in same time");
    }
}