namespace MainCore.Common.Errors.Storage
{
    public class GranaryLimit : Error
    {
        private GranaryLimit(long storage, long required) : base($"Don't have enough storage [{storage} < {required}]")
        {
        }

        public static GranaryLimit Error(long storage, long required) => new(storage, required);
    }
}