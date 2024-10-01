namespace MainCore.Common.Errors.AutoBuilder
{
    public class JobCompleted : Error
    {
        protected JobCompleted()
        {
        }

        public static JobCompleted Error => new();
    }
}