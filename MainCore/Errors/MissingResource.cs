namespace MainCore.Errors
{
    public class MissingResource : Error
    {
        private MissingResource(TroopEnums troop) : base($"Dont have enough resource to train {troop}")
        {
        }

        private MissingResource(string resource, long storage, long required) : base($"Don't have enough {resource}, need {required} but have {storage} ({required - storage})")
        {
        }

        public static MissingResource Error(TroopEnums troop) => new(troop);

        public static MissingResource Error(string resource, long storage, long required) => new(resource, storage, required);
    }
}