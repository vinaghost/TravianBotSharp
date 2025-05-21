namespace MainCore.Errors
{
    public class ResourceMissing : Error
    {
        protected ResourceMissing(string resource, long storage, long required) : base($"Don't have enough {resource}, need {required} but have {storage} ({required - storage})")
        {
        }

        public static ResourceMissing Error(string resource, long storage, long required) => new(resource, storage, required);

        public static ResourceMissing Wood(long storage, long required) => new("Wood", storage, required);

        public static ResourceMissing Clay(long storage, long required) => new("Clay", storage, required);

        public static ResourceMissing Iron(long storage, long required) => new("Iron", storage, required);

        public static ResourceMissing Crop(long storage, long required) => new("Crop", storage, required);
    }
}