namespace MainCore.Common.Errors.Storage
{
    public class StorageLimit(string type, long storage, long required) : Error
    {
        public static StorageLimit Error(string type, long storage, long required) => new(type, storage, required);

        public void Log(ILogger logger) => logger.Warning("{Type} doesn't have enough capacity [{Storage} < {Required}]", type, storage, required);
    }
}