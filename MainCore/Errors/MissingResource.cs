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

        public static MissingResource Wood(long storage, long required) => new("Wood", storage, required);

        public static MissingResource Clay(long storage, long required) => new("Clay", storage, required);

        public static MissingResource Iron(long storage, long required) => new("Iron", storage, required);

        public static MissingResource Crop(long storage, long required) => new("Crop", storage, required);
    }
}
