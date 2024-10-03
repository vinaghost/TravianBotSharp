namespace MainCore.Common.Errors.AutoBuilder
{
    public class Continue : Error
    {
        protected Continue()
        {
        }

        public static Continue Error => new();
    }
}