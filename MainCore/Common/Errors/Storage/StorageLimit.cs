namespace MainCore.Common.Errors.Storage
{
    public class StorageLimit : Error
    {
        protected StorageLimit(string type, long storage, long required) : base($"{type} doesn't have enough capacity")
        {
            WithMetadata("storage", storage);
            WithMetadata("required", required);
        }

        public static StorageLimit Error(string type, long storage, long required) => new(type, storage, required);
    }
}