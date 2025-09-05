namespace MainCore.Errors
{
    public class MissingBuilding : Error
    {
        private MissingBuilding(BuildingEnums building) : base($"{building} is missing.")
        {
        }

        public static MissingBuilding Error(BuildingEnums building) => new(building);
    }
}
