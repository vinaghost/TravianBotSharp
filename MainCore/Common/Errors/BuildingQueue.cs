using FluentResults;
using MainCore.Common.Enums;

namespace MainCore.Common.Errors
{
    public class BuildingQueue : Error
    {
        private BuildingQueue(string message) : base(message)
        {
        }

        public static BuildingQueue NotTaskInqueue => new($"There is no suitable task available in job queue");

        public static BuildingQueue Full => new("Amount of currently building is equal with maximum building can build in same time");

        public static BuildingQueue NotEnoughPrerequisiteBuilding(BuildingEnums building, BuildingEnums prerequisiteBuilding, int level) => new($"{prerequisiteBuilding} [{level}] is missing when constructing {building}");

        public static BuildingQueue NotEnoughPrerequisiteBuilding(BuildingEnums building, int level) => new($"first {building} [{level}] is not level max yet");
    }
}