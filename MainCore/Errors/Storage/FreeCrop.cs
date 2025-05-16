namespace MainCore.Errors.Storage
{
    public class FreeCrop : Error
    {
        protected FreeCrop(long storage, long required) : base("Don't have enough freecrop")
        {
            WithMetadata("storage", storage);
            WithMetadata("required", required);
        }

        public static FreeCrop Error(long storage, long required) => new(storage, required);
    }
}