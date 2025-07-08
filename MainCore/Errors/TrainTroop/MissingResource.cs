namespace MainCore.Errors.TrainTroop
{
    public class MissingResource : Error
    {
        private MissingResource(TroopEnums troop) : base($"Dont have enough resource to train {troop}")
        {
        }

        public static MissingResource Error(TroopEnums troop) => new(troop);
    }
}