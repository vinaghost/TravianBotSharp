namespace MainCore.Common.Errors.Storage
{
    public class Resource(string resource, long storage, long required) : Error
    {
        public static Resource Error(string resource, long storage, long required) => new(resource, storage, required);

        public void Log(ILogger logger) => logger.Warning("Don't have enough {Resource} [{Storage} < {Required}]", resource, storage, required);
    }
}