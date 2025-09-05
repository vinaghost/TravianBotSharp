namespace MainCore.Errors
{
    public class StorageLimit : Error
    {
        protected StorageLimit(string type, long storage, long required) : base($"{type} doesn't have enough capacity, need {required} but have {storage} ({required - storage})")
        {
        }

        public static StorageLimit Warehouse(long storage, long required) => new("Warehouse", storage, required);

        public static StorageLimit Granary(long storage, long required) => new("Granary", storage, required);
    }
}
