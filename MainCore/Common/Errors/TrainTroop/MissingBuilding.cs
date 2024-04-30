namespace MainCore.Common.Errors.TrainTroop
{
    public class MissingBuilding : Error
    {
        private MissingBuilding(BuildingEnums building) : base($"{building} is missing. Disable train troop on this building")
        {
        }

        public static MissingBuilding Error(BuildingEnums building) => new(building);
    }
}