namespace MainCore.Common.Errors.Storage
{
    public class WarehouseLimit : Error
    {
        private WarehouseLimit(long storage, long required) : base($"Don't have enough storage [{storage} < {required}]")
        {
        }

        public static WarehouseLimit Error(long storage, long required) => new(storage, required);
    }
}