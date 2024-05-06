namespace MainCore.Common.Errors.AutoBuilder
{
    public class BuildingQueue : Error
    {
        private BuildingQueue(string message) : base(message)
        {
        }

        public static BuildingQueue Full => new("Amount of currently building is equal with maximum building can build in same time");

        public static BuildingQueue NotEnoughPrerequisiteBuilding(BuildingEnums building, BuildingEnums prerequisiteBuilding, int level) => new($"{prerequisiteBuilding} [{level}] is missing when constructing {building}");

        public static BuildingQueue NotEnoughPrerequisiteBuilding(BuildingEnums building, int level) => new($"{building} [{level}] is not level max yet");
    }
}