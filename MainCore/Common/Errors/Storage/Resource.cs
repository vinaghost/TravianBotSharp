namespace MainCore.Common.Errors.Storage
{
    public class Resource : Error
    {
        private Resource(string resource, long storage, long required) : base($"Don't have enough {resource} [{storage} < {required}]")
        {
        }

        public static Resource Error(string resource, long storage, long required) => new(resource, storage, required);
    }
}