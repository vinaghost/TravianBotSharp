using FluentResults;
using MainCore.Common.Enums;

namespace MainCore.Common.Errors.TrainTroop
{
    public class MissingBuilding : Error
    {
        public MissingBuilding(BuildingEnums building) : base($"{building} is missing. Disable train troop on this building")
        {
        }
    }
}