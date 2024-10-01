namespace MainCore.Common.Errors.Storage
{
    public class FreeCrop(long storage, long required) : Error
    {
        public static FreeCrop Error(long storage, long required) => new(storage, required);

        public void Log(ILogger logger) => logger.Warning("Don't have enough freecrop [{Storage} < {Required}]", storage, required);
    }
}