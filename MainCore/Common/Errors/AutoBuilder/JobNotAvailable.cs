namespace MainCore.Common.Errors.AutoBuilder
{
    public class JobNotAvailable : Error
    {
        protected JobNotAvailable()
        {
        }

        public static JobNotAvailable Error => new();
    }
}