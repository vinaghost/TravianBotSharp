namespace MainCore.Common.Errors.AutoBuilder
{
    public sealed class Continue : Error
    {
        protected Continue()
        {
        }

        public static Continue Error => new();
    }
}