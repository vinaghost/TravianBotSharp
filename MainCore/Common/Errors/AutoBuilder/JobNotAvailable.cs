namespace MainCore.Common.Errors.AutoBuilder
{
    public class JobNotAvailable : Error
    {
        public JobNotAvailable(string type) : base($"{type} job is not available")
        {
        }

        public static JobNotAvailable Error(string type) => new(type);
    }
}