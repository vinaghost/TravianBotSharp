namespace MainCore.Common.Errors.Storage
{
    public class Resource : Error
    {
        protected Resource(string resource, long storage, long required) : base($"Don't have enough {resource}")
        {
            WithMetadata("storage", storage);
            WithMetadata("required", required);
        }

        public static Resource Error(string resource, long storage, long required) => new(resource, storage, required);
    }
}