namespace MainCore.Errors
{
    public class Continue : Error
    {
        protected Continue()
        {
        }

        public static Continue Error => new();
    }
}