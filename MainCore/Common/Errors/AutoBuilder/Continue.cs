namespace MainCore.Common.Errors.AutoBuilder
{
    public sealed class Continue : Error
    {
        private Continue()
        {
        }

        public static Continue Error => new();
    }
}